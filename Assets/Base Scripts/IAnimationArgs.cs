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
        AnimationType Type { get; }
        Action OnComplete { get; set; }
    }
    
    [Serializable]
    public struct ItemFlyArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.ITEM_FLY;
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
    
    [Serializable]
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
    
    [Serializable]
    public struct StatChangeTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.STAT_CHANGE_TEXT;
        public Action OnComplete { get; set; }

        public int Amount;
        public Color CustomColor;
        public TMP_FontAsset CustomFont;
        public Vector2 Offset;
        public Vector2 MoveDistance;
        public GameObject TargetObject;
    }
    
    [Serializable]
    public struct ScreenShakeArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SCREEN_SHAKE;
        public Action OnComplete { get; set; }

        public float Intensity;
        public float Duration;
        public AnimationCurve ShakeCurve;
    }
    
    [Serializable]
    public struct SimpleParticleArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SIMPLE_PARTICLE;
        public Action OnComplete { get; set; }

        // If true, interpret position as world space and convert to screen/UI space.
        public bool fromWorld;

        // Used when fromWorld == true
        public Vector3 worldPosition;
        public Camera worldCamera; // optional; falls back to Camera.main

        // Used when fromWorld == false
        public Vector2 anchoredPosition;

        // Optional custom parent (e.g., a specific canvas or UI layer)
        public Transform customParent;
        public Material customMaterial;
        public Color customColor;
        // Optional behavior
        public bool ignoreTimeScale;
    }

}