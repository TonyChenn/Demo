using NCore;
using NCore.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ThreadSync : MonoBehaviour
{
	[SerializeField] RawImage image;

	string url = "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png";

	public static SynchronizationContext UnityThreadContext { get; private set; }
	public static int UnityThreadID { get; private set; }

    void Start()
    {
		//UnityThreadContext = SynchronizationContext.Current;
		//UnityThreadID = Thread.CurrentThread.ManagedThreadId;

		//Debug.Log($"Unity线程ID为：{UnityThreadID}");

		//// 开启一个线程输出日志
		// await Task.Run(print1);

		string origin = "hello";
		Debug.Log($"回收前：{origin}");
		origin.Recycle();

		string str1 = "oll";
		string str2 = "eh";
		string result = StringPool.Concat(str1, str2);

		Debug.Log($"回收修改后{origin}");
		Debug.Log($"连接结果：{result}");
    }

	private void print1()
	{
		Debug.Log($"子线程内执行，线程ID为：{Thread.CurrentThread.ManagedThreadId}");

		UnityThreadContext.Post(async (state) =>
		{
			string info = $"刷新UI的线程ID为：{Thread.CurrentThread.ManagedThreadId}";
			Debug.Log(info);
			var result = await WebServer.GetTexture(url);
			image.texture = result.GetTexture();
		}, null);
	}

	private async void print2()
	{
		Debug.Log($"子线程内执行，线程ID为：{Thread.CurrentThread.ManagedThreadId}");

		string info = $"刷新UI的线程ID为：{Thread.CurrentThread.ManagedThreadId}";
		Debug.Log(info);
		var result = await WebServer.GetTexture(url);
		image.texture = result.GetTexture();
	}
}
