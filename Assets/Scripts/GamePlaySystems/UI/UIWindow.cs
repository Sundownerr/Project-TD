using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    public enum Move { Down, Up }

    protected float[] defaultYs;

    public virtual void Open()
    {
        for (int i = 0; i < defaultYs.Length; i++)
            transform.GetChild(i).DOLocalMoveY(0, NumberConsts.UITransitionAnimationSpeed).SetEase(Ease.InOutQuint);
    }

    public virtual void Close(Move moveTo)
    {
        for (int i = 0; i < defaultYs.Length; i++)
            transform.GetChild(i).DOLocalMoveY(
                moveTo == Move.Up ? defaultYs[i] : -defaultYs[i], 
                NumberConsts.UITransitionAnimationSpeed)
                .SetEase(Ease.InOutQuint);
    }
}
