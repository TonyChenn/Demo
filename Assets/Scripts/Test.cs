using NDebug;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.U2D;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private Button btnToast;
    private Button btnDialog;
    private void Start()
    {
        btnToast = GameObject.Find("BtnToast").GetComponent<Button>();
        btnToast.onClick.AddListener(async () =>
        {
            NSDK.Toast.Show("测试Toast");
            await Task.Delay(1000);
            Log.Info("等待1s后输出");
        });

        btnDialog = GameObject.Find("BtnDialog").GetComponent<Button>();
        btnDialog.onClick.AddListener(() =>
        {
            NSDK.Dialog.Show("提示", "这是提示消息");
        });
    }
}
