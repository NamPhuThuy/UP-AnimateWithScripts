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

        private readonly float _holdDuration = 0.3f;
        private readonly float _inDuration = 0.3f;
        private readonly float _outDuration = 0.5f;
        private readonly float _upDistance = 0.02f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;
        [SerializeField] private ToastType toastType = ToastType.FLOAT_IN_MIDDLE;

        [Header("Positioning & Sizing")]
        [SerializeField] private Vector2 screenPercentage = new Vector2(0.5f, 0.35f);
        [SerializeField] private Vector2 backgroundSizePercent;

        private Canvas _defaultCanvas;
        private RectTransform _defaultCanvasRect;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRect;
        private Sequence _seq;
        private Vector2 _defaultSizeDelta;
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
            _defaultSizeDelta = _rectTransform.sizeDelta;
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
                HandleSpamClick();
                
                SetValues();
                
                PlayAnim();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_Toast");
            }
        }

        public void HandleSpamClick()
        {
            KillTweens();
            _seq?.Kill(false);
            _seq = null;
            _rectTransform.DOKill(false);
            _canvasGroup.DOKill(false);
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

            Vector2 percent = currentArgs.screenPercentage != Vector2.zero
                ? currentArgs.screenPercentage
                : screenPercentage;

            SetAnchoredPos(AnimationConst.GetAnchoredPosFromPercent(_parentCanvasRect, percent));

            Vector2 bgSizePercent = currentArgs.backgroundSizePercent != Vector2.zero
                ? currentArgs.backgroundSizePercent
                : backgroundSizePercent;

            SetBackgroundSize(bgSizePercent);
            
            float targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            _rectTransform.localScale = Vector3.one * targetScale;
            if (backImage.rectTransform != _rectTransform)
            {
                backImage.rectTransform.localScale = Vector3.one * targetScale;
            }
            
            SetContent(currentArgs.message);
        }

        protected override void ResetValues()
        {
            _seq = null;
            gameObject.SetActive(false);
            _canvasGroup.alpha = 0f;
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
            SetBackgroundSize(Vector2.zero);
            _rectTransform.localScale = Vector3.one;
        }

        #endregion
        private void PlayAnim()
        {
            _seq?.Kill(false);

            float holdTime = currentArgs.customHoldDuration > 0f ? currentArgs.customHoldDuration : _holdDuration;

            float upDistPercent = currentArgs.customUpDistance > 0f ? currentArgs.customUpDistance : _upDistance;
            float upDist = upDistPercent <= 1f ? upDistPercent * _parentCanvasRect.rect.height : upDistPercent;

            ToastType activeType = currentArgs.toastType != ToastType.NONE ? currentArgs.toastType : toastType;

            switch (activeType)
            {
                case ToastType.FLOAT_ALL:
                    PlayFloatAllAnim(holdTime, upDist);
                    break;
                case ToastType.FLOAT_IN_MIDDLE:
                    PlayFloatInMiddleAnim(holdTime, upDist);
                    break;
                case ToastType.FLASH:
                default:
                    PlayFlashAnim(holdTime, upDist);
                    break;
            }
        }

        private void PlayFlashAnim(float holdTime, float upDist)
        {
            float targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f * targetScale, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Join(_canvasGroup.DOFade(1f,  _inDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f * targetScale, 0.3f * _inDuration).SetEase(_inEase));
            
            if (holdTime > 0f) _seq.AppendInterval(holdTime);
            
            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, _inDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f * targetScale, 0.3f * _outDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _outDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _outDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatAllAnim(float holdTime, float upDist)
        {
            float targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            _rectTransform.localScale = Vector3.one * targetScale;
            _canvasGroup.alpha = 0f;

            float totalDuration = _inDuration + holdTime + _outDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, _canvasGroup.DOFade(1f, _inDuration).SetEase(_inEase));
            _seq.Insert(_inDuration + holdTime, _canvasGroup.DOFade(0f, _outDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatInMiddleAnim(float holdTime, float upDist)
        {
            float targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            _rectTransform.localScale = Vector3.one * targetScale;
            _canvasGroup.alpha = 0f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_canvasGroup.DOFade(1f, _inDuration).SetEase(_inEase));
            if (holdTime > 0f)
            {
                _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, holdTime).SetEase(_upEase));
            }
            else
            {
                _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, _inDuration).SetEase(_upEase));
            }
            _seq.Append(_canvasGroup.DOFade(0f, _outDuration).SetEase(_downEase));
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

        public void SetBackgroundSize(Vector2 sizePercent)
        {
            if (sizePercent == Vector2.zero)
            {
                _rectTransform.sizeDelta = _defaultSizeDelta;
                backImage.rectTransform.sizeDelta = _defaultSizeDelta;
                return;
            }

            Vector2 targetSize = AnimationConst.GetSizeFromPercent(_parentCanvasRect, sizePercent);
            if (targetSize.x <= 0f) targetSize.x = _defaultSizeDelta.x;
            if (targetSize.y <= 0f) targetSize.y = _defaultSizeDelta.y;

            _rectTransform.sizeDelta = targetSize;
            backImage.rectTransform.sizeDelta = targetSize;
        }

        #endregion

        #region Getters

        

        #endregion
        
    }
}