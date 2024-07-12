using NCore;
using UnityEngine;

public class WaitPanel : MonoBehaviour
{
	[SerializeField] Transform BG;

	private static int counter = 0;
	private void Awake()
	{
		BG.SetActive(false);
	}

	public void Show(bool show)
	{
		if (show) { ++counter; }
		else
		{
			--counter;
			if (counter < 0) counter = 0;
		}

		BG.SetActive(counter > 0);
	}

	public void HideForce()
	{
		counter = 0;
		BG.SetActive(false);
	}
}
