using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnTableViewCell(UITableViewCell cell);

[RequireComponent(typeof(ScrollRect))]
public class UITableView : MonoBehaviour
{
	public enum MoveDirection { TopToBottom, BottomToTop, LeftToRight, RightToLeft }

	[SerializeField] MoveDirection direction = MoveDirection.TopToBottom;
	[SerializeField] float startOffset = 0;
	[SerializeField] float endOffset = 0;

	[HideInInspector] public RectTransform CachedRectTrans { get; private set; }
	[HideInInspector] public RectTransform CachedRectContent { get; private set; }
	[HideInInspector] public ITableViewDelegate Delegate { get; set; }

	bool isFirstReload = true;
	const float threshold = 0.1f;


	public event OnTableViewCell eventCellWillAppear;
	public event OnTableViewCell eventCellWillDisappear;

	float offsetY = 0;

	private ScrollRect scrollRect;

	List<UITableViewCell> usedCells = new List<UITableViewCell>(8);
	List<UITableViewCell> unUsedCells = new List<UITableViewCell>(8);

	private void Awake()
	{
		CachedRectTrans = GetComponent<RectTransform>();
		scrollRect = GetComponent<ScrollRect>();
		CachedRectContent = scrollRect.content;

		scrollRect.onValueChanged.AddListener(OnScrollChanged);
	}

	private void OnDestroy()
	{
		usedCells.ForEach(item => { if (item != null) { Destroy(item.gameObject); } });
		unUsedCells.ForEach(item => { if (item != null) { Destroy(item.gameObject); } });
		usedCells.Clear();
		unUsedCells.Clear();

		eventCellWillAppear = null;
		this.eventCellWillDisappear = null;
	}

	private void Start()
	{
		isFirstReload = false;
	}

	private void ResetData()
	{
		offsetY = 0;
	}

	private int indexFromOffset(float offset, ref Vector2 pos)
	{
		int num = Delegate.NumberOfCells(this);

		float wh = 0;

		switch (direction)
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
			case MoveDirection.BottomToTop:
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

		eventCellWillAppear?.Invoke(cell);

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
		eventCellWillDisappear?.Invoke(cell);

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
			if (direction == MoveDirection.TopToBottom) xy = pos.y - startOffset;
			else if (direction == MoveDirection.BottomToTop) xy = pos.y - startOffset;
			else if (direction == MoveDirection.LeftToRight) xy = pos.x + startOffset;

			for (int i = from; i <= to; ++i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 size = Delegate.SizeForIndex(this, i);

					if (direction == MoveDirection.TopToBottom)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					else if (direction == MoveDirection.BottomToTop)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					else if (direction == MoveDirection.LeftToRight)
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
			if (direction == MoveDirection.TopToBottom) xy = _cell.RectTrans.anchoredPosition.y;
			else if (direction == MoveDirection.BottomToTop) xy = _cell.RectTrans.anchoredPosition.y;
			else if (direction == MoveDirection.LeftToRight) xy = _cell.RectTrans.anchoredPosition.x;

			for (int i = begin - 1; i >= from; --i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 size = Delegate.SizeForIndex(this, i);
					if (direction == MoveDirection.TopToBottom)
					{
						xy += size.y;
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
					}
					else if (direction == MoveDirection.BottomToTop)
					{
						xy += size.y;
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
					}
					else if (direction == MoveDirection.LeftToRight)
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
			if (direction == MoveDirection.TopToBottom)
			{
				xy = _cell.RectTrans.anchoredPosition.y;
				xy -= size.y;
			}
			else if (direction == MoveDirection.BottomToTop)
			{
				xy = _cell.RectTrans.anchoredPosition.y;
				xy -= size.y;
			}
			else if (direction == MoveDirection.LeftToRight)
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

					if (direction == MoveDirection.TopToBottom)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					if (direction == MoveDirection.BottomToTop)
					{
						cell.RectTrans.anchoredPosition = new Vector2(0, xy);
						xy -= size.y;
					}
					else if (direction == MoveDirection.LeftToRight)
					{
						cell.RectTrans.anchoredPosition = new Vector2(xy, 0);
						xy += size.x;
					}
				}
			}
		}
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
			if (direction == MoveDirection.TopToBottom) wh += itemSize.y;
			else if (direction == MoveDirection.BottomToTop) wh += itemSize.y;
			else if (direction == MoveDirection.LeftToRight) wh += itemSize.x;
		}
		wh += startOffset;
		wh += endOffset;

		Vector2 size = CachedRectContent.sizeDelta;
		if (direction == MoveDirection.TopToBottom) size.y = wh;
		else if (direction == MoveDirection.BottomToTop) size.y = wh;
		else if (direction == MoveDirection.LeftToRight) size.x = wh;

		CachedRectContent.sizeDelta = size;

		float xy = 0;
		float maxXY = 0;
		if (direction == MoveDirection.TopToBottom)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (direction == MoveDirection.BottomToTop)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (direction == MoveDirection.LeftToRight)
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
			if (direction == MoveDirection.TopToBottom) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (direction == MoveDirection.BottomToTop) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (direction == MoveDirection.LeftToRight) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.x, ref posTo);

			removeUnvisibleCells(from, to);
			showVisibleCells(from, to, posFrom);
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
			if (direction == MoveDirection.TopToBottom)
			{
				x = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
			}
			else if (direction == MoveDirection.BottomToTop)
			{
				x = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
			}
			else if (direction == MoveDirection.LeftToRight)
			{
				y = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
			}

			if (Delegate != null)
			{
				int num = Delegate.NumberOfCells(this);
				for (int i = 0; i < num; ++i)
				{
					Vector2 size = Delegate.SizeForIndex(this, i);
					if (direction == MoveDirection.TopToBottom) y += size.y;
					else if (direction == MoveDirection.BottomToTop) y += size.y;
					else if (direction == MoveDirection.LeftToRight) x += size.x;
				}
				if (direction == MoveDirection.TopToBottom) y -= CachedRectTrans.sizeDelta.y;
				else if (direction == MoveDirection.BottomToTop) y -= CachedRectTrans.sizeDelta.y;
				else if (direction == MoveDirection.LeftToRight) x -= CachedRectTrans.sizeDelta.x;

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
	public void OnScrollChanged(Vector2 pos)
	{
		float xy = 0;
		float maxXY = 0;
		if (direction == MoveDirection.TopToBottom)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (direction == MoveDirection.BottomToTop)
		{
			xy = CachedRectContent.anchoredPosition.y;
			maxXY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;
		}
		else if (direction == MoveDirection.LeftToRight)
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
			if (direction == MoveDirection.TopToBottom) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (direction == MoveDirection.BottomToTop) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);
			else if (direction == MoveDirection.LeftToRight) to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.x, ref posTo);

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
