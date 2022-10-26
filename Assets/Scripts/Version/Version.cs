using NCore;
using SFramework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class Version : NormalSingleton<Version>
{
    private Version() { }

    public async Task<string> GetRemoteVersion()
    {
        ChannelItem channel = ChannelConfig.Singleton.CurChannel;
        Hashtable data = new Hashtable();
        data.Add("op_id", "2106");
        data.Add("opgame_id", "1");
        UnityWebRequest request = await WebServer.Post($"{channel.ServerURL}/ver",data);
        if(request.result == UnityWebRequest.Result.Success)
        {
            JSONNode node = request.GetJsonNode();
            return node["ver"];
        }
        Debug.LogError("获取远程版本失败");
        return null;
    }
    public string GetLocalVersion()
    {
        return "123.456";
    }
    public void SetLocalVersion()
    {

    }

    public void Download() { }
}
