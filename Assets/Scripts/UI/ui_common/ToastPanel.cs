using System.Threading.Tasks;
using TweenKit;
using UnityEngine;
using UnityEngine.UI;

public class ToastPanel : MonoBehaviour
{
	[SerializeField] TweenRotation tween;
	[SerializeField] Text tip;

	private void Start()
	{
		tween.def = new Vector3 (90, 0, 0);
		tween.from = new Vector3(90, 0, 0);
		tween.to = Vector3.zero;
	}

	public async void Show(string text, int millisecondsDelay = 1000)
	{
		if(millisecondsDelay < 0) millisecondsDelay = 1000;

		tip.text = text;
		tween.Play(true);
		await Task.Delay(millisecondsDelay);
		tween.Play(false);
	}
}
