using NextFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ص�ԭ��
/// </summary>
public class BundleUnit
{
    public string Name;
    public string Hash;
    public int Length;
    public BundleState State;

    public bool NeedDownload
    {
        get
        {
            return State == BundleState.Add || State == BundleState.Modify;
        }
    }

    public BundleUnit(string name, string hash, int length)
    {
        Name = name;
        Hash = hash;
        Length = length > 0 ? length : 1024 * 100;
        State = BundleState.None;
    }
}

/// <summary>
/// bundle ״̬
/// </summary>
public enum BundleState
{
    None,
    Add,        // ��
    Remove,     // ɾ
    Modify,     // ��
    NoChange,   // �ޱ仯
}
