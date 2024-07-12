//-------------------------------------------------
//            TweenKit
// Copyright © 2020 tonychenn.cn
//-------------------------------------------------

namespace TweenKit
{
    public enum Trigger
    {
        OnClick,
        OnHover,
        OnPress,
        OnHoverTrue,
        OnHoverFalse,
        OnPressTrue,
        OnPressFalse,
        OnActivate,
        OnActivateTrue,
        OnActivateFalse,
        OnDoubleClick,
        OnSelect,
        OnSelectTrue,
        OnSelectFalse,
        Manual,
    }

    public enum Direction
    {
        Reverse = -1,
        Toggle = 0,
        Forward = 1,
    }

    public enum EnableCondition
    {
        DoNothing = 0,
        EnableThenPlay,
        IgnoreDisabledState,
    }

    public enum DisableCondition
    {
        DisableAfterReverse = -1,
        DoNotDisable = 0,
        DisableAfterForward = 1,
    }
}
