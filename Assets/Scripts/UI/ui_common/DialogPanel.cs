using NCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
	[SerializeField] Image Mask;
	[SerializeField] Text Title;
	[SerializeField] Text Content;
	[SerializeField] Button BtnClose;

	[SerializeField] Transform OneButtonGroup;
	[SerializeField] Button OneYesClick;
	[SerializeField] Text OneYesText;

	[SerializeField] Transform TwoButtonGroup;
	[SerializeField] Button TwoYesClick;
	[SerializeField] Text TwoYesText;
	[SerializeField] Button TwoNoClick;
	[SerializeField] Text TwoNoText;

	Action yesClickCallBack = null;
	Action noClickCallBack = null;

	private string defaultTitleText;
	private string defaultYesText;
	private string defaultNoText;
	private void Awake()
	{
		Mask.gameObject.SetActive(false);
		BtnClose.onClick.AddListener(NoClickHandler);
		OneYesClick.onClick.AddListener(YesClickHandler);
		TwoYesClick.onClick.AddListener(YesClickHandler);
		TwoNoClick.onClick.AddListener(NoClickHandler);

		defaultTitleText = Localization.Get("提示");
		defaultYesText = Localization.Get("确定");
		defaultNoText = Localization.Get("取消");
	}

	public void ShowDialog(string title, string content, Action yesAction, string strYes, Action noAction, string strNo, bool mask = true)
	{
		title ??= defaultTitleText;
		strYes ??= defaultYesText;
		strNo ??= defaultNoText;


		Mask.gameObject.SetActive(mask);
		if (yesAction == null && noAction == null)
		{
			OneButtonGroup.gameObject.SetActive(false);
			TwoButtonGroup.gameObject.SetActive(false);
		}
		else if (yesAction != null && noAction == null)
		{
			OneButtonGroup.gameObject.SetActive(true);
			OneYesText.text = strYes;
			TwoButtonGroup.gameObject.SetActive(false);
			yesClickCallBack = yesAction;
		}
		else if (yesAction != null && noAction != null)
		{
			OneButtonGroup.gameObject.SetActive(false);
			TwoButtonGroup.gameObject.SetActive(true);
			TwoYesText.text = strYes;
			TwoNoText.text = strNo;
			yesClickCallBack = yesAction;
			noClickCallBack = noAction;
		}
		Title.text = title;
		Content.text = content;
	}

	private void YesClickHandler()
	{
		yesClickCallBack?.Invoke();
		Mask.gameObject.SetActive(false);
	}
	private void NoClickHandler()
	{
		noClickCallBack?.Invoke();
		Mask.gameObject.SetActive(false);
	}
}



