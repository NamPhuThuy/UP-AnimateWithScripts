/*
Github: https://github.com/NamPhuThuy
*/

using System;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public interface IAnimationArgs
    {
        //interface members are implicitly public
        public AnimationType Type { get; }
        public Action OnComplete { get; set; }
    }
    
    public struct ItemFlyArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.ITEM_FLY; //interface implementation
        public Action OnComplete { get; set; }

        [Tooltip("The value to add to the preValue")]
        public int AddValue;
        
        [Tooltip("The value before adding")]
        public int PrevValue;
        public int ItemAmount;
        public float DelayBetweenItems;
        
        [Tooltip("Transform that contain TextMeshProUGUI")]
        public Transform Target;
        public Transform TargetInteractTransform;
        public Sprite ItemSprite;
        public Vector3 StartPosition;
        public System.Action OnItemInteract;
    }
    
    public struct PopupTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.POPUP_TEXT;
        public Action OnComplete { get; set; }

        public string Message;
        public Vector3 CustomAnchoredPos;
        public Transform CustomParent;
        public float CustomScale;
        public Color TextColor; // default is (0f, 0f, 0f, 0f)
        public TMP_FontAsset TextFont; 
        public float Duration;
    }
    
    public struct StatChangeTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.STAT_CHANGE_TEXT;
        public Action OnComplete { get; set; }

        public int Amount;
        public Color Color;
        public Vector2 Offset;
        public Vector2 MoveDistance;
        public Transform InitialParent;
    }
    
    public struct ScreenShakeArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SCREEN_SHAKE;
        public Action OnComplete { get; set; }

        public float Intensity;
        public float Duration;
        public AnimationCurve ShakeCurve;
    }
    
}