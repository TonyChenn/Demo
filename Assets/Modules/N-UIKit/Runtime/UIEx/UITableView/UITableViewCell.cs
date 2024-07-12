using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
