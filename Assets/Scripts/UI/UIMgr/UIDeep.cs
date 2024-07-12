using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIDeep
{
    ButtomLayer     = 1,        // 最底层(备用)
    CommonLayer     = 2,        // 普通层
    UnderPopLayer   = 3,        // 弹框层之下
    PopLayer        = 4,        // 弹框层
    UpPopLayer      = 5,        // 弹框层之上
    TopLayer        = 6,        // 顶层(锁屏，loading等)
}
