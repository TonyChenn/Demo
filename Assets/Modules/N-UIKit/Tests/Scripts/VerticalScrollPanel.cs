using UnityEngine;

public class VerticalScrollPanel : MonoBehaviour, ITableViewDelegate
{
	[SerializeField] UITableView tableView;
	[SerializeField] VerticalScrollItem itemPrefab;


	private void Start()
	{
		itemPrefab.gameObject.SetActive(false);

		tableView.SetTableViewDelegate(this);
		tableView.ReloadData();
	}

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

	public Vector2 SizeForIndex(UITableView tableview, int index)
	{
		return itemPrefab.GetComponent<RectTransform>().sizeDelta;
	}

	public int NumberOfCells(UITableView tableview)
	{
		return 100;
	}

	public void OnScrollChanged(Vector2 pos)
	{
	}
}
