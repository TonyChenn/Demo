using NCore;
using System;
using UnityEngine;

/// <summary>
/// 本地化
/// </summary>
public static class Localization
{
	/// <summary>
	/// 本地化语言类型
	/// </summary>
	public enum LanguageEnum
	{
		Auto = int.MinValue,
		Chinese = 6,
		English = 10,
	}

	private static LanguageEnum _language;
	public static void Init()
	{
		LanguageEnum curLanguage = GetLanguage();
		if(curLanguage == LanguageEnum.Auto)
		{
			if(Enum.IsDefined(typeof(LanguageEnum), Application.systemLanguage))
			{
				_language = (LanguageEnum)Application.systemLanguage;
				return;
			}
			_language = LanguageEnum.Chinese;
			return;
		}
		_language = curLanguage;
	}
	public static string Get(string key)
	{
		return key;
	}

	#region PlayerPrefs
	public static void SetLanguage(LanguageEnum lang)
	{
		PlayerPrefsHelper.SetEnum("Localization_Language", lang);
	}
	public static LanguageEnum GetLanguage()
	{
		return PlayerPrefsHelper.GetEnum<LanguageEnum>("Localization_Language", LanguageEnum.Chinese);
	}
	#endregion
}
