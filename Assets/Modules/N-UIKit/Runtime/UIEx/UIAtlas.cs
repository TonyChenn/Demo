using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAtlas : ScriptableObject
{
	[SerializeField] public Texture2D texture;
	[SerializeField] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
	

	public Sprite GetSprite(string spriteName)
	{
		return sprites[spriteName];
	}
}
