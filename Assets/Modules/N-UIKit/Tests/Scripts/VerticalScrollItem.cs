using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollItem : UITableViewCell
{
	[SerializeField] Text text;

	public override void UpdateCell(int index)
	{
		text.text = index.ToString();
	}
}
