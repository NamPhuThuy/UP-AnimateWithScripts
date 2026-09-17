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
        public TextMeshProUGUI MessageText => messageText;
        [SerializeField] private Image backImage;

        private readonly float _inDuration = 0.35f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.5f;
        private readonly float _upDistance = 24f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;
        [SerializeField] private ToastType toastType = ToastType.FLASH;
        [SerializeField] private bool isChangeColor = false;
        [SerializeField] private bool isCustomUpDistance = false;
        [SerializeField] private float customUpDistance = 24f;
        [SerializeField] private bool isCustomHoldDuration = false;
        [SerializeField] private float customHoldDuration = 0.8f;

        public ToastType ToastType
        {
            get => toastType;
            set => toastType = value;
        }

        public bool IsChangeColor
        {
            get => isChangeColor;
            set => isChangeColor = value;
        }

        public bool IsCustomUpDistance
        {
            get => isCustomUpDistance;
            set => isCustomUpDistance = value;
        }

        public float CustomUpDistance
        {
            get => customUpDistance;
            set => customUpDistance = value;
        }

        public bool IsCustomHoldDuration
        {
            get => isCustomHoldDuration;
            set => isCustomHoldDuration = value;
        }

        public float CustomHoldDuration
        {
            get => customHoldDuration;
            set => customHoldDuration = value;
        }

        private Canvas _defaultCanvas;
        private RectTransform _defaultCanvasRect;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRect;
        private Sequence _seq;
        private Vector2 _basePos;
        private Color _defaultBackColor;
        private readonly string _fallbackText = "Readying!";
       

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
            if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
            if (backImage) _defaultBackColor = backImage.color;
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
          
            if (currentArgs.customParent != null)
            {
                transform.parent = currentArgs.customParent.transform;
                _parentCanvas = GetComponentInParent<Canvas>();
                _parentCanvasRect = _parentCanvas.GetComponent<RectTransform>();
            }

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
            
            if (isChangeColor || currentArgs.isChangeColor)
            {
                SetRandomColor();
            }
            else if (backImage != null)
            {
                backImage.color = _defaultBackColor;
            }

            if (currentArgs.textColor != default) 
            {
                messageText.color = currentArgs.textColor;
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
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
        }

        #endregion
        private void PlayAnim()
        {
            _seq?.Kill(false);

            float holdTime = currentArgs.isCustomHoldDuration
                ? currentArgs.customHoldDuration
                : (isCustomHoldDuration ? customHoldDuration : _holdDuration);

            float upDist = currentArgs.isCustomUpDistance
                ? currentArgs.customUpDistance
                : (isCustomUpDistance ? customUpDistance : _upDistance);

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

            if (currentArgs.customDuration != 0f)
                StartAutoReturn(currentArgs.customDuration);
        }

        private void PlayFlashAnim(float holdTime, float upDist)
        {
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f, 0.3f * _inDuration).SetEase(_inEase));
            
            if (holdTime > 0f) _seq.AppendInterval(holdTime);
            
            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, _upDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatAnim(float holdTime, float upDist)
        {
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            float totalDuration = _inDuration + holdTime + _downFadeDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, _canvasGroup.DOFade(1f, _inDuration).SetEase(_inEase));
            _seq.Insert(0f, _rectTransform.DOScale(Vector3.one, _inDuration).SetEase(_inEase));
            _seq.Insert(_inDuration + holdTime, _canvasGroup.DOFade(0f, _downFadeDuration).SetEase(_downEase));
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