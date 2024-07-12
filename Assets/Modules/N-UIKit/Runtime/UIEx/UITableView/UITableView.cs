using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnTableViewCell(UITableViewCell cell);

[RequireComponent(typeof(ScrollRect))]
public class UITableView : MonoBehaviour
{
	public enum MoveDirection { LeftToRight, TopToBottom }

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

	int minIndex = 0;
	int maxIndex = 0;

	ScrollRect scrollRect;

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
		for (int i = 0; i < unUsedCells.Count; ++i)
		{
			if (unUsedCells[i] != null)
			{
				Destroy(unUsedCells[i].gameObject);
			}
		}
		unUsedCells.Clear();

		eventCellWillAppear = null;
		this.eventCellWillDisappear = null;
	}

	private void Start()
	{
		ReloadData();
		isFirstReload = false;
	}

	private void ResetData()
	{
		offsetY = 0;
	}

	private int indexFromOffset(float offset, ref Vector2 pos)
	{
		uint num = Delegate.NumberOfCells(this);

		float h = 0;
		pos.y = 0;
		for (int i = 0; i < num; i++)
		{
			pos.y = -h;
			Vector2 size = Delegate.SizeForIndex(this, i);
			h += size.y;

			if (h > offset) return i;
		}
		pos.y = -h;
		if (num > 0)
		{
			return ((int)(num - 1));
		}
		return -1;
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
		if (from != -1) minIndex = from;
		if (to != -1) maxIndex = to;

		if (usedCells.Count == 0 && from != -1 && to != -1)
		{
			float y = pos.y - startOffset;
			for (int i = from; i <= to; ++i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);

					cell.RectTrans.anchoredPosition = new Vector2(0, y);

					Vector2 size = Delegate.SizeForIndex(this, i);
					float h = size.y;
					y -= h;
				}
			}
			return;
		}

		if (from != -1)
		{
			UITableViewCell _cell = usedCells[0];
			int begin = _cell.Index;
			float y = _cell.RectTrans.anchoredPosition.y;
			for (int i = begin - 1; i >= from; --i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);
					Vector2 size = Delegate.SizeForIndex(this, i);
					y += size.y;

					cell.RectTrans.anchoredPosition = new Vector2(0, y);
				}
			}
		}
		if (to != -1)
		{
			UITableViewCell _cell = usedCells[usedCells.Count - 1];
			int end = _cell.Index;
			float y = _cell.RectTrans.anchoredPosition.y;

			Vector2 size = Delegate.SizeForIndex(this, end);
			y -= size.y;
			for (int i = end + 1; i <= to; ++i)
			{
				UITableViewCell cell = Delegate.CellForIndex(this, i);
				if (cell)
				{
					insertCellAtIndex(cell, i);

					cell.RectTrans.anchoredPosition = new Vector2(0, y);

					Vector2 cellSize = Delegate.SizeForIndex(this, end);
					y -= cellSize.y;
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

		float height = 0;
		uint num = Delegate.NumberOfCells(this);
		for (int i = 0; i < num; ++i)
		{
			Vector2 itemSize = Delegate.SizeForIndex(this, i);
			height += itemSize.y;
		}
		height += startOffset;
		height += endOffset;

		Vector2 size = CachedRectContent.sizeDelta;
		size.y = height;
		CachedRectContent.sizeDelta = size;


		float y = CachedRectContent.anchoredPosition.y;
		float maxY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;

		if (y < threshold)
		{
			y = threshold;
		}
		else if (y > (maxY + threshold))
		{
			y = maxY + threshold;
		}

		if (!Mathf.Approximately(offsetY, y))
		{
			offsetY = y;

			Vector2 posTo = Vector2.zero;
			Vector2 posFrom = Vector2.zero;
			int from = indexFromOffset(offsetY, ref posFrom);
			int to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);

			removeUnvisibleCells(from, to);
			showVisibleCells(from, to, posFrom);
		}
	}

	public UITableViewCell HeadCellInSight()
	{
		if (usedCells.Count > 0)
		{
			return usedCells[0];
		}
		return null;
	}

	public UITableViewCell TailCellInSight()
	{
		if (usedCells.Count > 1)
		{
			return usedCells[usedCells.Count - 1];
		}
		return HeadCellInSight();
	}

	public UITableViewCell CellAtIndex(int index)
	{
		if (usedCells.Count > 0)
		{
			UITableViewCell cell = usedCells[0];
			if (index == cell.Index)
			{
				return cell;
			}
			else if (index < cell.Index)
			{
				return null;
			}
			if (usedCells.Count > 1)
			{
				cell = usedCells[usedCells.Count - 1];
				if (index == cell.Index)
				{
					return cell;
				}
				else if (index > cell.Index)
				{
					return null;
				}

				for (int i = 1; i < usedCells.Count - 1; ++i)
				{
					cell = usedCells[i];
					if (cell.Index == index)
					{
						return cell;
					}
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
		if (offset.x > max.x)
		{
			offset.x = max.x;
		}
		if (offset.y > max.y)
		{
			offset.y = max.y;
		}
		CachedRectContent.anchoredPosition = offset;
	}

	public Vector2 MaxOffset()
	{
		if (isFirstReload)
		{
			float x = CachedRectContent.sizeDelta.x - CachedRectTrans.sizeDelta.x;
			float y = 0;
			if (Delegate != null)
			{
				uint num = Delegate.NumberOfCells(this);
				for (int i = 0; i < num; ++i)
				{
					Vector2 size = Delegate.SizeForIndex(this, i);
					y += size.y;
				}
				y -= CachedRectTrans.sizeDelta.y;
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
		float y = CachedRectContent.anchoredPosition.y;
		float maxY = CachedRectContent.sizeDelta.y - CachedRectTrans.sizeDelta.y;

		if (y < threshold)
		{
			y = threshold;
		}
		else if (y > (maxY + threshold))
		{
			y = maxY + threshold;
		}

		if (!Mathf.Approximately(offsetY, y))
		{
			offsetY = y;

			Vector2 posTo = Vector2.zero;
			Vector2 posFrom = Vector2.zero;
			int from = indexFromOffset(offsetY, ref posFrom);
			int to = indexFromOffset(offsetY + CachedRectTrans.sizeDelta.y, ref posTo);

			removeUnvisibleCells(from, to);
			showVisibleCells(from, to, posFrom);
		}

		Delegate.OnScrollChanged(CachedRectContent.anchoredPosition);
	}

}

public interface ITableViewDelegate
{
	uint NumberOfCells(UITableView tableview);
	UITableViewCell CellForIndex(UITableView tableview, int index);
	Vector2 SizeForIndex(UITableView tableview, int index);
	void OnScrollChanged(Vector2 pos);
}
