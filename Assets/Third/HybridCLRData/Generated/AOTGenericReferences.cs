public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// N-Core.dll
	// System.Core.dll
	// UnityEngine.AndroidJNIModule.dll
	// UnityEngine.AssetBundleModule.dll
	// UnityEngine.CoreModule.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// NCore.AwaitExtension.SimpleCoroutineAwaiter<object>
	// NCore.NormalSingleton<object>
	// System.Action<byte>
	// System.Action<float>
	// System.Action<object>
	// System.Action<ulong,ulong,float>
	// System.Collections.Generic.Dictionary<UIFlag,object>
	// System.Collections.Generic.Dictionary<object,ulong>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<UnityEngine.Color>
	// System.Collections.Generic.List<UIFlag>
	// System.Collections.Generic.List<RenderHeads.Media.AVProVideo.MediaPlayer.PlatformOptions.HTTPHeader>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<UnityEngine.UIVertex>
	// System.Collections.Generic.List<RenderHeads.Media.AVProVideo.Stream.Chunk>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Func<int,object>
	// System.Func<float,float>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<byte>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.TaskAwaiter<byte>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.Task<byte>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<object,RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType,RenderHeads.Media.AVProVideo.ErrorCode>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<object,RenderHeads.Media.AVProVideo.StreamParserEvent.EventType>
	// UnityEngine.Events.UnityEvent<object,RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType,RenderHeads.Media.AVProVideo.ErrorCode>
	// }}

	public void RefMethods()
	{
		// object[] System.Array.Empty<object>()
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,object>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<byte>.AwaitOnCompleted<object,Version.<DownloaRemoteMD5File>d__13>(object&,Version.<DownloaRemoteMD5File>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Version.<GetRemoteVersion>d__12>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Version.<GetRemoteVersion>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<byte>.Start<Version.<DownloaRemoteMD5File>d__13>(Version.<DownloaRemoteMD5File>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<Version.<GetRemoteVersion>d__12>(Version.<GetRemoteVersion>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitOnCompleted<object,UpdatePanel.<<Start>b__4_1>d>(object&,UpdatePanel.<<Start>b__4_1>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UpdatePanel.<<Start>b__4_0>d>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UpdatePanel.<<Start>b__4_0>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Version.<CheckUpdate>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Version.<CheckUpdate>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<byte>,Version.<CheckUpdate>d__10>(System.Runtime.CompilerServices.TaskAwaiter<byte>&,Version.<CheckUpdate>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Version.<CheckUpdate>d__10>(System.Runtime.CompilerServices.TaskAwaiter&,Version.<CheckUpdate>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<UpdatePanel.<<Start>b__4_1>d>(UpdatePanel.<<Start>b__4_1>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<UpdatePanel.<<Start>b__4_0>d>(UpdatePanel.<<Start>b__4_0>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Version.<CheckUpdate>d__10>(Version.<CheckUpdate>d__10&)
		// object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object[] UnityEngine.GameObject.GetComponents<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Transform)
		// object[] UnityEngine.Resources.FindObjectsOfTypeAll<object>()
	}
}