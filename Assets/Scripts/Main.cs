using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main
{
    public static void Run()
    {
        GameObject go = GameObject.Find("Main");

        if (go == null)
        {
            go = new GameObject("Main");
        }
        var comp = go.GetComponent<Test>();

        if(comp == null)
        {
            go.AddComponent<Test>();
        }
    }
}
