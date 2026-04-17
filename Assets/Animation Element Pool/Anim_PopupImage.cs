using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_PopupImage : AnimationBase
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private PopupImageArgs currentArgs;
        [SerializeField] private Image image;
        [SerializeField] private RectTransform imageRectTransform;

        
        [Header("Stats")]
        [SerializeField] private Vector2 basePos;

        [Header("Flags")] 
        [SerializeField] private bool isAdjustAnchoredPos;
        #endregion

        #region Private Fields

        private Sequence _seq;
        
        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.5f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.35f;
        private readonly float _upDistance = 18f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;
        
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void PlayAnim()
        {
            _seq?.Kill(false);
           
            imageRectTransform.localScale = Vector3.zero;

            _seq = DOTween.Sequence().SetUpdate(false);

            _seq.Append(imageRectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(imageRectTransform.DOScale(1f, 0.3f * _inDuration).SetEase(_inEase));
            
            if (_holdDuration > 0f) _seq.AppendInterval(_holdDuration);
            
            _seq.Append(imageRectTransform.DOAnchorPosY(imageRectTransform.anchoredPosition.y + _upDistance, _upDuration).SetEase(_upEase));
            _seq.Append(imageRectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(image.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(imageRectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(() =>
            {
                ResetValues();
                Recycle();
                try
                {
                    currentArgs.OnComplete?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnComplete callback: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private void SetPosition()
        {
            if (currentArgs.useScreenPercentage)
            {
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                    
                    // Instead of changing anchors, we calculate the offset from the current anchors
                    // This way we respect the prefab's setup and center pivot
                    
                    // Convert percentage to anchored position space based on canvas size
                    float targetX = canvasRect.rect.width * (currentArgs.screenPercentage.x / 100f);
                    float targetY = canvasRect.rect.height * (currentArgs.screenPercentage.y / 100f);

                    // Since standard anchors are middle/center, the bottom left is (-width/2, -height/2)
                    // We need to shift the target position so (50,50) is (0,0) locally
                    float finalX = targetX - (canvasRect.rect.width * 0.5f);
                    float finalY = targetY - (canvasRect.rect.height * 0.5f);

                    imageRectTransform.anchoredPosition = new Vector2(finalX, finalY);
                }
            }
            else if (currentArgs.isUseAnchoredPos)
            {
                imageRectTransform.anchoredPosition = currentArgs.anchoredPos;
            }
            else
            {
                imageRectTransform.position = Camera.main.WorldToScreenPoint(currentArgs.customPosition);
            }
        }
        
        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is PopupImageArgs popupImageArgs)
            {
                currentArgs = popupImageArgs;
                gameObject.SetActive(true);
                SetValues();
                SetPosition();
                PlayAnim();
            }
        }

        protected override void SetValues()
        {
            image.sprite = currentArgs.sprite;
            image.SetNativeSize();
            Color tempColor = image.color;
            tempColor.a = 1f;
            image.color = tempColor;
            
            // Custom values
            if (currentArgs.customFilterColor != default)
            {
                image.color = currentArgs.customFilterColor;
            }
        }

        protected override void ResetValues()
        {
            
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(Anim_PopupSprite))]
    [CanEditMultipleObjects]
    public class Anim_PopupSpriteEditor : Editor
    {
        private Anim_PopupSprite script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
            script = (Anim_PopupSprite)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
           

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}