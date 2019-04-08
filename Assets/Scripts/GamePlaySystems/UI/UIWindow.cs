using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    public enum Move { Down, Up }

    protected float[] defaultYs = new float[10];

    public virtual void Open()
    {
        transform.GetChild(0).DOLocalMoveY(0,  NumberConsts.UITransitionAnimationSpeed).SetEase(Ease.InOutQuint);
    }

    public virtual void Close(Move moveTo)
    {
        transform.GetChild(0).DOLocalMoveY(moveTo == Move.Up ? defaultYs[0] : -defaultYs[0],  NumberConsts.UITransitionAnimationSpeed).SetEase(Ease.InOutQuint);
    }
}
