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
    
    public struct ItemFlyArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.ITEM_FLY;
        public Action OnComplete { get; set; }

        [Tooltip("The value to add to the preValue")]
        public int addValue;
        
        [Tooltip("The value before adding")]
        public int prevValue;
        public int itemAmount;
        public float delayBetweenItems;
        
        [Tooltip("Transform that contain TextMeshProUGUI")]
        public Transform target;
        public Transform targetInteractTransform;
        public Sprite itemSprite;
        public Vector3 startPosition;
        public System.Action onItemInteract;
    }
    
    public struct PopupTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.POPUP_TEXT;
        public Action OnComplete { get; set; }

        public string message;
        public Vector3 customAnchoredPos;
        public Transform customParent;
        public float customScale;
        public Color textColor; // default is (0f, 0f, 0f, 0f)
        public TMP_FontAsset textFont; 
        public float duration;
    }
    
    public struct StatChangeTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.STAT_CHANGE_TEXT;
        public Action OnComplete { get; set; }

        public int amount;
        public Color color;
        public Vector2 offset;
        public Vector2 moveDistance;
        public Transform initialParent;
    }
    
    public struct ScreenShakeArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SCREEN_SHAKE;
        public Action OnComplete { get; set; }

        public float intensity;
        public float duration;
        public AnimationCurve shakeCurve;
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

        // Optional behavior
        public bool ignoreTimeScale;
    }

}