using System;

[AttributeUsage(AttributeTargets.Method)]
public class SceneViewMenuAttribute : Attribute
{
	public string Title { get; private set; }

	public SceneViewMenuAttribute(string title)
	{
		Title = title;
	}
}
