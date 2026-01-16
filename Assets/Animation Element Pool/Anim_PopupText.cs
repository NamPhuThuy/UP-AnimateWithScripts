using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Anim_PopupText : AnimationBase
    {
        [Header("Stats")] 
        [SerializeField] private float duration = 1f;
        [SerializeField] private PopupTextArgs currentArgs;
        
        [Header("Components")]
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI messageText;
        public TextMeshProUGUI MessageText => messageText;
        [SerializeField] private Image backImage;

        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.35f;
        private readonly float _upDistance = 18f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;

        private Sequence _seq;
        private Vector2 _basePos;
        private readonly string _fallbackText = "Watch out!";
       

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
            if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
            _basePos = _rectTransform.anchoredPosition;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is PopupTextArgs popupArgs)
            {
                currentArgs = popupArgs;
                gameObject.SetActive(true);
                KillTweens();
                
                SetValues();
                
                PlayAnim();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXPopupText");
            }
        }

        protected override void SetValues()
        {
            if (currentArgs.TextFont != null)
            {
                messageText.font = currentArgs.TextFont; // Apply custom font
            }
            else
            {
                messageText.font = AnimationManager.Ins.DefaultFont;
            }
            
            if (currentArgs.CustomParent != null)
            {
                transform.parent = currentArgs.CustomParent.transform;
            }

            duration = Mathf.Max(currentArgs.Duration, duration);

            if (currentArgs.CustomAnchoredPos != default)
            {
                SetAnchoredPos(currentArgs.CustomAnchoredPos);
            }
            else
            {
                SetAnchoredPos(_basePos);
            }
            
            if (!Mathf.Approximately(currentArgs.CustomScale, 0f))
            {
                backImage.rectTransform.localScale = Vector3.one * currentArgs.CustomScale;
                messageText.rectTransform.localScale = Vector3.one * currentArgs.CustomScale;
                DebugLogger.Log(message:$"use custom");
            }
            else
            {
                backImage.rectTransform.localScale = Vector3.one;
                messageText.rectTransform.localScale = Vector3.one;
                DebugLogger.Log(message:$"dont use custom");
            }
            
            SetContent(currentArgs.Message);
            SetRandomColor();

            if (currentArgs.TextColor != default) 
            {
                messageText.color = currentArgs.TextColor;
            }
            else
            {
                messageText.color = Color.white;
            }
        }

        protected override void ResetValues()
        {
            _seq = null;
            gameObject.SetActive(false);
            _rectTransform.anchoredPosition = _basePos;
            _canvasGroup.alpha = 0f;
        }

        #endregion
        private void PlayAnim()
        {
            _seq?.Kill(false);
           
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f, 0.3f * _inDuration).SetEase(_inEase));
            
            if (_holdDuration > 0f) _seq.AppendInterval(_holdDuration);
            
            _seq.Append(_rectTransform.DOAnchorPosY(_basePos.y + _upDistance, _upDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(() =>
            {
                ResetValues();
                Recycle();
                currentArgs.OnComplete?.Invoke();
            });
            
            StartAutoReturn(duration + 0.5f);
        }
        
        #region Set Up
        
        public void SetContent(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = _fallbackText;
            }
            messageText.text = message;
        }

        public void SetContent(string message, Action moreSetup)
        {
            messageText.text = message;
            moreSetup?.Invoke();
        }

        private void SetRandomColor()
        {
            var colorPairs = ColorHelper.RandomContrastColorPair();
            backImage.color = colorPairs.Key;
            // messageText.color = colorPairs.Value;
        }
       
        private void SetAnchoredPos(Vector2 anchoredPos)
        {
            _rectTransform.anchoredPosition = anchoredPos;
        }

        #endregion

        #region Getters

        

        #endregion
        
    }
}
