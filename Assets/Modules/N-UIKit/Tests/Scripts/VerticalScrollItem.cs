using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VerticalScrollItem : UITableViewCell
{
	[SerializeField] TextMeshProUGUI text;

	public override void UpdateCell(int index)
	{
		text.text = index.ToString();
	}
}
