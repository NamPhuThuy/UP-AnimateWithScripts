/*
Github: https://github.com/NamPhuThuy/UP-AnimateWithScripts
*/

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        //Events
        public Action OnComplete { get; set; }
        public System.Action OnItemInteract;

        [Tooltip("The value to add to the preValue")]
        public int addValue;
        
        [Tooltip("The value before adding")]
        public int prevValue;
        public int itemAmount;
        public float delayBetweenItems;
        
        [Tooltip("Transform that contain TextMeshProUGUI")]
        public Transform targetText;
        public Transform targetInteractTransform;
        public Sprite itemSprite;
        public Vector3 startPosition;
    }
    
    public enum ToastType
    {
        NONE = 0,
        FLASH = 1,
        FLOAT = 2,
    }

    [Serializable]
    public struct ToastArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.TOAST;
        public Action OnComplete { get; set; }

        // Must-have Values
        public string message;
        public Color textColor; // default is (0f, 0f, 0f, 0f)
        public TMP_FontAsset textFont;
        
        // Custom Values
        public ToastType toastType;
        public float customDuration;
        public Vector3 customAnchoredPos;
        public Transform customParent;
        public float customScale;
        public bool customEnableBackImage;
        public bool isChangeColor;
        public bool isCustomUpDistance;
        public float customUpDistance;
        public bool isCustomHoldDuration;
        public float customHoldDuration;
        
        // New percentage-based positioning
        public bool useScreenPercentage;
        public Vector2 screenPercentage; // e.g., (50, 50) for center
    }

    [Serializable]
    public struct PopupImageArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.POPUP_IMAGE;
        public Action OnComplete { get; set; }
        
        // Must have values
        public Sprite sprite;
        
        // Positioning
        public bool isUseAnchoredPos;
        public Vector2 anchoredPos;
        
        public bool useScreenPercentage;
        public Vector2 screenPercentage; // e.g., (50, 50) for center

        // Custom values
        public Color customFilterColor;
        public float customDuration;
        public Vector2 customPosition;
    }
    
    [Serializable]
    public struct ToastWorldSpaceArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.TOAST_WORLD_SPACE;
        public Action OnComplete { get; set; }

        // Must-have Values
        public string message;
        public Vector3 worldPosition;

        // Target / Offset
        public Transform targetTransform;
        public Vector3 worldOffset;
        public Camera customCamera;

        // Styling
        public Color textColor;
        public TMP_FontAsset textFont;
        public float textSize;

        // Custom Values
        public ToastType toastType;
        public float customDuration;
        public float customScale;
        public bool isChangeColor;
        public bool isCustomUpDistance;
        public float customUpDistance;
        public bool isCustomHoldDuration;
        public float customHoldDuration;
    }

    [Serializable]
    public struct StatChangeTextArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.STAT_CHANGE_TEXT;
        
        // Events
        public Action OnComplete { get; set; }

        public int amount;
        public string additionalIconText; // e.g., "%"
        public bool isBold;
        public float duration;
        public Color customColor;
        public bool isUseAnchoredPos;
        
        public Vector2 rectTransformOffset;
        public Vector2 moveDistance;
        public GameObject targetObject;
        
        //Custom Values
        public TMP_FontAsset customFont;
    }
    
    [Serializable]
    public struct ScreenShakeArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SCREEN_SHAKE;
        public Action OnComplete { get; set; }

        public float intensity;
        public float duration;
        public AnimationCurve shakeCurve;
    }
    
    [Serializable]
    public struct SpriteMotionArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SPRITE_MOTION;
        public Action OnComplete { get; set; }

        // Must have values
        public Sprite sprite;
        public Vector3 worldSpaceStartPosi;
        public ObjActiveAuto.MotionType motionType;
      

        // Custom values
        public float customDuration;
        public float customVerticalSize;
        public float customDelay;
        public string customSortingLayer;
        public int customSortingOrder;
    }

    [Serializable]
    public struct RewardProgressArgs : IAnimationArgs
    {
        public AnimationType Type => AnimationType.SEGMENT_REWARD_PROGRESS;
        public Action OnComplete { get; set; }
        public Action<int> OnSegmentComplete { get; set; }  // Callback with segment index
        
        // Must have values
        public Sprite prizeSprite;
        public int totalSegmentNum;
        public int currentSegmentNum;
        public int targetSegmentNum;
        
        // Option values
        public string customMessage;
        public Sprite customSegmentSprite;
        public Sprite customBackgroundSprite;
        public Sprite customEndpointSprite;
        
        // Animation settings
        public float segmentFillDuration;
        public float delayBetweenSegments;
        public bool playSound;
        public bool showParticles;
    }
   
}