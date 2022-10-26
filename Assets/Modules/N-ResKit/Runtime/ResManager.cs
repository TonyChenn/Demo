using NCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResManager
{
    // AssetBundle³Ø
    public static Dictionary<string, AssetBundle> AllBundlePool = new Dictionary<string, AssetBundle>(32);


    private static Dictionary<string, AssetBundle> AllResPool = new Dictionary<string, AssetBundle>(16);

    public static AssetBundle GetAssetBundle(string bundleName)
    {
        return AllBundlePool[bundleName];
    }

}
