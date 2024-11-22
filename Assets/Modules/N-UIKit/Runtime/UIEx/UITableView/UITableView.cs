using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class UITableViewCell : MonoBehaviour
{
	[HideInInspector] public RectTransform RectTrans { get; private set; }
	[HideInInspector] public int Index { get; set; }


	protected void Awake()
	{
		RectTrans = GetComponent<RectTransform>();
		Index = -1;
	}

	public abstract void UpdateCell(int index);
}

[APIInfo("N-UGUI", "UITableView", @"移植自cocos2dx中cc.TableView组件。
支持高性能大量数据展示，支持：
1. ListView横向/竖向滚动
2. GridView横向/竖向滚动
3. 节点大小不固定横向/竖向滚动

# API文档
- SetTableViewDelegate
- OnCellWillAppear
- OnCellWillDisappear
- ReloadData
- ReloadDataInPos

# 横向滚动示例：
```csharp
public class HorizontalScrollPanel : MonoBehaviour, ITableViewDelegate
{
	[SerializeField] UITableView tableView;
	[SerializeField] VerticalScrollItem itemPrefab;

	private void Start()
	{
		// 绑定事件
		tableView.SetTableViewDelegate(this);
		// 刷新UI
		tableView.ReloadData();
	}

	// 根据索引显示item
	public UITableViewCell CellForIndex(UITableView tableview, int index)
	{
		UITableViewCell item = tableView.DequeueCell();
		if (item == null)
		{
			item = Instantiate(itemPrefab, tableview.CachedRectContent, false);
			item.gameObject.SetActive(true);
		}
		item.UpdateCell(index);
		return item;
	}

	// 返回item的大小
	public Vector2 SizeForIndex(UITableView tableview, int index)
	{
		return itemPrefab.GetComponent<RectTransform>().sizeDelta;
	}

	// item的数量
	public int NumberOfCells(UITableView tableview)
	{
		return 100;
	}

	public void OnScrollChanged(Vector2 pos) { }
}
```
")]
[RequireComponent(typeof(ScrollRect))]
public class UITableView : MonoBehaviour
{
	public enum MoveDirection { TopToBottom, LeftToRight }

	[SerializeField] MoveDirection m_MoveDirection = MoveDirection.TopToBottom;
	[SerializeField] int m_RowOrColumCount = 2;

	[SerializeField] float m_StartOffset = 0;
	[SerializeField] float m_EndOffset = 0;

	[HideInInspector] public RectTransform CachedRectTrans{ get; private set; }
	[HideInInspector] public RectTransform CachedRectContent {  get; private set; }
	
	private ScrollRect scrollRect;

	const float threshold = 0.1f;

	private bool isFirstReload = true;
	private float offsetY = 0;

	private Action<UITableViewCell> cellWillAppearAction;
	private Action<UITableViewCell> cellWillDisappearAction;
	private ITableViewDelegate Delegate;


	List<UITableViewCell> usedCells = new List<UITableViewCell>(8);
	List<UITableViewCell> unUsedCells = new List<UITableViewCell>(8);

	private void Awake()
	{
		CachedRectTrans = GetComponent<RectTransform>();
		scrollRect = GetComponent<ScrollRect>();
		CachedRectContent = scrollRect.content;

		scrollRect.onValueChanged.AddListener(onScrollChanged);
	}

	private void Start()
	{
		isFirstReload = false;
	}

	private void OnDestroy()
	{
		usedCells.ForEach(item => { if (item != null) { Destroy(item.gameObject); } });
		unUsedCells.ForEach(item => { if (item != null) { Destroy(item.gameObject); } });
		usedCells.Clear();
		unUsedCells.Clear();

		cellWillAppearAction = null;
		cellWillDisappearAction = null;
	}


	#region API
	public void SetTableViewDelegate(ITableViewDelegate dele)
	{
		Delegate = dele;
	}
	public void OnCellWillAppear(Action<UITableViewCell> action)
	{
		if (action != null) cellWillAppearAction += action;
	}
	public void OnCellWillDisappear(Action<UITableViewCell> action)
	{
		if (action != null) cellWillDisappearAction += action;
	}

	public void ReloadDataInPos()
	{
		Vector2 pos = CachedRectContent.anchoredPosition;
		ReloadData();
		CachedRectContent.anchoredPosition = pos;
	}
	public void ReloadData()
	{
		ResetData();
		scrollRect.StopMovement();

		for (int i = usedCells.Count - 1; i >= 0; --i)
		{
			UITableViewCell cell = usedCells[i];
			moveCellOutOfSight(cell);
		}

		float wh = 0;
		int num = Delegate.NumberOfCells(this);
		for (int i = 0; i < num; ++i)
		{
			Vector2 itemSize = Delegate.SizeForIndex(this, i);
			if (m_MoveDirection == MoveDirection.TopToBottom) wh += itemSize.y;
			else if (m_MoveDirection == MoveDirection.LeftToRight) wh += itemSize.x;
		}
		wh += m_StartOffset;
		wh += m_EndOffset;

		Vector2 size = CachedRectContent.sizeDelta;
		if (m_MoveDirection == MoveDirection.TopToBottom) size.y = wh;
		else if (m_MoveDirection == MoveDirection.LeftToRight) size.x = wh;

		CachedRectContent.sizeDelta = size;

		float xy = 0;
		float maxXY = 0;
		if (m_MoveDirection == MoveDirection.TopToBottom)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (m_MoveDirection == MoveDirection.LeftToRight)
		{
			xy = CachedRectContent.anchoredPosition.x;
			maxXY = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
		}

		xy = Math.Clamp(xy, threshold, maxXY + threshold);

		if (!Mathf.Approximately(offsetY, xy))
		{
			offsetY = xy;

			Vector2 posTo = Vector2.zero;
			Vector2 posFrom = Vector2.zero;
			int from = indexFromOffset(offsetY, ref posFrom);
			int to = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (m_MoveDirection == MoveDirection.LeftToRight) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.x, ref posTo);

			removeUnvisibleCells(from, to);
			showVisibleCells(from, to, posFrom);
		}
	}
	#endregion

	private void ResetData()
	{
		offsetY = 0;
	}

	private int indexFromOffset(float offset, ref Vector2 pos)
	{
		int num = Delegate.NumberOfCells(this);

		float wh = 0;

		switch (m_MoveDirection)
		{
			case MoveDirection.TopToBottom:
				pos.y = 0;
				for (int i = 0; i < num; i++)
				{
					pos.y = -wh;
					Vector2 size = Delegate.SizeForIndex(this, i);
					wh += size.y;
					if (wh > offset) return i;
				}
				pos.y = -wh;
				break;
			case MoveDirection.LeftToRight:
				pos.x = 0;
				for (int i = 0; i < num; i++)
				{
					pos.x = wh;
					Vector2 size = Delegate.SizeForIndex(this, i);
					wh += size.x;
					if (wh > offset) return i;
				}
				pos.x = wh;
				break;
		}

		return num > 0 ? (num - 1) : -1;
	}

	private void insertCellAtIndex(UITableViewCell cell, int index)
	{
		cell.Index = index;
		cell.enabled = true;

		cellWillAppearAction?.Invoke(cell);

		cell.transform.SetParent(CachedRectContent.transform, false);

		for (int i = usedCells.Count - 1; i >= 0; --i)
		{
			UITableViewCell _cell = usedCells[i];
			if (cell.Index > _cell.Index)
			{
				usedCells.Insert(i + 1, cell);
				return;
			}
		}
		usedCells.Insert(0, cell);
	}

	private void moveCellOutOfSight(UITableViewCell cell)
	{
		cellWillDisappearAction?.Invoke(cell);

		cell.Index = -1;
		cell.transform.SetParent(null, false);
		cell.enabled = false;

		unUsedCells.Add(cell);
		usedCells.Remove(cell);
	}

	private void removeUnvisibleCells(int from, int to)
	{
		if (usedCells.Count > 0)
		{
			if (-1 != from)
			{
				for (int i = usedCells.Count - 1; i >= 0; --i)
				{
					UITableViewCell cell = usedCells[i];
					if (cell.Index < from)
					{
						moveCellOutOfSight(cell);
					}
				}
			}
			if (-1 != to)
			{
				for (int i = usedCells.Count - 1; i >= 0; --i)
				{
					UITableViewCell cell = usedCells[i];
					if (cell.Index > to)
					{
						moveCellOutOfSight(cell);
					}
				}
			}
		}
	}

	private void showVisibleCells(int from, int to, Vector2 pos)
	{
		if (usedCells.Count == 0 && from != -1 && to != -1)
		{
			float xy = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom) xy = pos.y - m_StartOffset;
			else if (m_MoveDirection == MoveDirection.LeftToRight) xy = pos.x + m_StartOffset;

			for (int i = from; i <= to; ++i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 size = Delegate.SizeForIndex(this, i);

					if (m_MoveDirection == MoveDirection.TopToBottom)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					else if (m_MoveDirection == MoveDirection.LeftToRight)
					{
						cell.RectTrans.anchoredPosition = new Vector2(xy, 0);
						xy += size.x;
					}
				}
			}
			return;
		}

		if (from != -1)
		{
			UITableViewCell _cell = usedCells[0];
			int begin = _cell.Index;

			float xy = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom) xy = _cell.RectTrans.anchoredPosition.y;
			else if (m_MoveDirection == MoveDirection.LeftToRight) xy = _cell.RectTrans.anchoredPosition.x;

			for (int i = begin - 1; i >= from; --i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 size = Delegate.SizeForIndex(this, i);
					if (m_MoveDirection == MoveDirection.TopToBottom)
					{
						xy += size.y;
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
					}
					else if (m_MoveDirection == MoveDirection.LeftToRight)
					{
						xy -= size.x;
						cell.RectTrans.anchoredPosition = new Vector2(xy, 0);
					}
				}
			}
		}
		if (to != -1)
		{
			UITableViewCell _cell = usedCells[usedCells.Count - 1];
			int end = _cell.Index;
			Vector2 size = Delegate.SizeForIndex(this, end);

			float xy = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom)
			{
				xy = _cell.RectTrans.anchoredPosition.y;
				xy -= size.y;
			}
			else if (m_MoveDirection == MoveDirection.LeftToRight)
			{
				xy = _cell.RectTrans.anchoredPosition.x;
				xy += size.x;
			}

			for (int i = end + 1; i <= to; ++i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 cellSize = Delegate.SizeForIndex(this, end);

					if (m_MoveDirection == MoveDirection.TopToBottom)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					else if (m_MoveDirection == MoveDirection.LeftToRight)
					{
						cell.RectTrans.anchoredPosition = new Vector2(xy, 0);
						xy += size.x;
					}
				}
			}
		}
	}

	public UITableViewCell HeadCellInSight()
	{
		return usedCells.Count > 0 ? usedCells[0] : null;
	}

	public UITableViewCell TailCellInSight()
	{
		return usedCells.Count > 1 ? usedCells[usedCells.Count - 1] : HeadCellInSight();
	}

	public UITableViewCell CellAtIndex(int index)
	{
		if (usedCells.Count > 0)
		{
			UITableViewCell cell = usedCells[0];
			if (index == cell.Index) return cell;
			else if (index < cell.Index) return null;

			if (usedCells.Count > 1)
			{
				cell = usedCells[usedCells.Count - 1];

				if (index == cell.Index) return cell;
				else if (index > cell.Index) return null;

				for (int i = 1; i < usedCells.Count - 1; ++i)
				{
					cell = usedCells[i];
					if (cell.Index == index) return cell;
				}
			}
		}
		return null;
	}

	public UITableViewCell DequeueCell()
	{
		if (unUsedCells.Count > 0)
		{
			UITableViewCell cell = unUsedCells[unUsedCells.Count - 1];
			unUsedCells.RemoveAt(unUsedCells.Count - 1);
			return cell;
		}
		return null;
	}

	public Vector2 GetOffset()
	{
		return CachedRectContent.anchoredPosition;
	}

	public void ScrollToOffset(Vector2 offset)
	{
		Vector2 max = MaxOffset();
		offset.x = Math.Min(offset.x, max.x);
		offset.y = Math.Min(offset.y, max.y);

		CachedRectContent.anchoredPosition = offset;
	}

	public Vector2 MaxOffset()
	{
		if (isFirstReload)
		{
			float x = 0;
			float y = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom)
			{
				x = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
			}
			else if (m_MoveDirection == MoveDirection.LeftToRight)
			{
				y = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
			}

			if (Delegate != null)
			{
				int num = Delegate.NumberOfCells(this);
				for (int i = 0; i < num; ++i)
				{
					Vector2 size = Delegate.SizeForIndex(this, i);
					if (m_MoveDirection == MoveDirection.TopToBottom) y += size.y;
					else if (m_MoveDirection == MoveDirection.LeftToRight) x += size.x;
				}
				if (m_MoveDirection == MoveDirection.TopToBottom) y -= CachedRectTrans.sizeDelta.y;
				else if (m_MoveDirection == MoveDirection.LeftToRight) x -= CachedRectTrans.sizeDelta.x;

			}
			return new Vector2(x, y);
		}
		else
		{
			float x = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
			float y = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
			return new Vector2(x, y);
		}
	}

	//scrollrect event
	private void onScrollChanged(Vector2 pos)
	{
		float xy = 0;
		float maxXY = 0;
		if (m_MoveDirection == MoveDirection.TopToBottom)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (m_MoveDirection == MoveDirection.LeftToRight)
		{
			xy = -CachedRectContent.anchoredPosition.x;
			maxXY = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
		}

		xy = Math.Clamp(xy, threshold, maxXY + threshold);

		if (!Mathf.Approximately(offsetY, xy))
		{
			offsetY = xy;

			Vector2 posTo = Vector2.zero;
			Vector2 posFrom = Vector2.zero;
			int from = indexFromOffset(offsetY, ref posFrom);

			int to = 0;
			if (m_MoveDirection == MoveDirection.TopToBottom) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (m_MoveDirection == MoveDirection.LeftToRight) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.x, ref posTo);

			removeUnvisibleCells(from, to);
			showVisibleCells(from, to, posFrom);
		}

		Delegate.OnScrollChanged(CachedRectContent.anchoredPosition);
	}
}

public interface ITableViewDelegate
{
	int NumberOfCells(UITableView tableview);
	UITableViewCell CellForIndex(UITableView tableview, int index);
	Vector2 SizeForIndex(UITableView tableview, int index);
	void OnScrollChanged(Vector2 pos);
}
