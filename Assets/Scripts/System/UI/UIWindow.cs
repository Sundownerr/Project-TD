using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Consts;
using UnityEngine;

namespace Game.UI
{
    public class UIWindow : MonoBehaviour
    {
        public enum Move { Down, Up }

        protected float[] defaultYs;

        public virtual void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            for (int i = 0; i < defaultYs.Length; i++)
            {
                transform.GetChild(i).DOLocalMoveY(0, NumberConsts.UIAnimSpeed).SetEase(Ease.InOutQuint);
            }
        }

        public virtual void Close(Move moveTo, float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            for (int i = 0; i < defaultYs.Length; i++)
            {
                transform.GetChild(i).DOLocalMoveY(moveTo == Move.Up ? defaultYs[i] : -defaultYs[i], NumberConsts.UIAnimSpeed)
                    .SetEase(Ease.InOutQuint);
            }
        }
    }
}