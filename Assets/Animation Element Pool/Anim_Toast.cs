using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Anim_Toast : AnimationBase
    {
        [Header("Stats")] 
        [SerializeField] private ToastArgs currentArgs;
        
        [Header("Components")]
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backImage;

        private readonly float _holdDuration = 0.6f;
        private readonly float _upDuration = 0.5f;
        private readonly float _downDuration = 0.5f;
        private readonly float _upDistance = 36f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;
        [SerializeField] private ToastType toastType = ToastType.FLASH;

        private Canvas _defaultCanvas;
        private RectTransform _defaultCanvasRect;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRect;
        private Sequence _seq;
        private Vector2 _basePos;
        private readonly string _fallbackText = "Readying!";
       

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
            if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
            _defaultCanvas = GetComponentInParent<Canvas>();
            _defaultCanvasRect = _defaultCanvas.GetComponent<RectTransform>();
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
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
            if (args is ToastArgs popupArgs)
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
            if (currentArgs.textFont != null)
            {
                messageText.font = currentArgs.textFont; // Apply custom font
            }
          
            /*
            if (currentArgs.customParent != null)
            {
                transform.parent = currentArgs.customParent.transform;
                _parentCanvas = GetComponentInParent<Canvas>();
                _parentCanvasRect = _parentCanvas.GetComponent<RectTransform>();
            }*/

            if (currentArgs.useScreenPercentage)
            {
                // Instead of changing anchors, we calculate the offset from the current anchors
                // This way we respect the prefab's setup and center pivot
                float targetX = _parentCanvasRect.rect.width * (currentArgs.screenPercentage.x / 100f);
                float targetY = _parentCanvasRect.rect.height * (currentArgs.screenPercentage.y / 100f);

                // Since standard anchors are middle/center, the bottom left is (-width/2, -height/2)
                // We need to shift the target position so (50,50) is (0,0) locally
                float finalX = targetX - (_parentCanvasRect.rect.width * 0.5f);
                float finalY = targetY - (_parentCanvasRect.rect.height * 0.5f);

                SetAnchoredPos(new Vector2(finalX, finalY));
            }
            else if (currentArgs.customAnchoredPos != default)
            {
                SetAnchoredPos(currentArgs.customAnchoredPos);
            }
            else
            {
                SetAnchoredPos(_basePos);
            }
            
            if (!Mathf.Approximately(currentArgs.customScale, 0f))
            {
                backImage.rectTransform.localScale = Vector3.one * currentArgs.customScale;
                messageText.rectTransform.localScale = Vector3.one * currentArgs.customScale;
            }
            else
            {
                backImage.rectTransform.localScale = Vector3.one;
                messageText.rectTransform.localScale = Vector3.one;
            }
            
            SetContent(currentArgs.message);
        }

        protected override void ResetValues()
        {
            _seq = null;
            gameObject.SetActive(false);
            _rectTransform.anchoredPosition = _basePos;
            _canvasGroup.alpha = 0f;
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
        }

        #endregion
        private void PlayAnim()
        {
            _seq?.Kill(false);

            float holdTime;
            if (currentArgs.customHoldDuration > 0f)
            {
                holdTime = currentArgs.customHoldDuration;
            }
            else
            {
                holdTime = _holdDuration;
            }
          
            float upDist;
            if (currentArgs.customUpDistance > 0f)
            {
                upDist = currentArgs.customUpDistance;
            }
            else
            {
                upDist = _upDistance;
            }

            ToastType activeType = currentArgs.toastType != ToastType.NONE ? currentArgs.toastType : toastType;

            switch (activeType)
            {
                case ToastType.FLOAT:
                    PlayFloatAnim(holdTime, upDist);
                    break;
                case ToastType.FLASH:
                default:
                    PlayFlashAnim(holdTime, upDist);
                    break;
            }
        }

        private void PlayFlashAnim(float holdTime, float upDist)
        {
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _upDuration).SetEase(_inEase));
            _seq.Join(_canvasGroup.DOFade(1f,  _upDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f, 0.3f * _upDuration).SetEase(_inEase));
            
            if (holdTime > 0f) _seq.AppendInterval(holdTime);
            
            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, _upDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatAnim(float holdTime, float upDist)
        {
            _rectTransform.localScale = Vector3.one;
            _canvasGroup.alpha = 0f;

            float totalDuration = _upDuration + holdTime + _downDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, _canvasGroup.DOFade(1f, _upDuration).SetEase(_inEase));
            _seq.Insert(_upDuration + holdTime, _canvasGroup.DOFade(0f, _downDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void OnAnimationComplete()
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