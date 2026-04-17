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

        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.35f;
        private readonly float _upDistance = 24f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;

        private Sequence _seq;
        private Vector2 _basePos;
        private readonly string _fallbackText = "Readying!";
       

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
            }

            if (currentArgs.useScreenPercentage)
            {
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                    
                    // Instead of changing anchors, we calculate the offset from the current anchors
                    // This way we respect the prefab's setup and center pivot
                    float targetX = canvasRect.rect.width * (currentArgs.screenPercentage.x / 100f);
                    float targetY = canvasRect.rect.height * (currentArgs.screenPercentage.y / 100f);

                    // Since standard anchors are middle/center, the bottom left is (-width/2, -height/2)
                    // We need to shift the target position so (50,50) is (0,0) locally
                    float finalX = targetX - (canvasRect.rect.width * 0.5f);
                    float finalY = targetY - (canvasRect.rect.height * 0.5f);

                    SetAnchoredPos(new Vector2(finalX, finalY));
                }
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
            SetRandomColor();

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
            
            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + _upDistance, _upDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));
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
            
            if (currentArgs.customDuration != 0f)
                StartAutoReturn(currentArgs.customDuration);
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