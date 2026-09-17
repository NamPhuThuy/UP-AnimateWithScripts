using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_Toast_Image : AnimationBase
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private ToastImageArgs currentArgs;
        [SerializeField] private Image image;
        public Image Image => image;
        [SerializeField] private RectTransform imageRectTransform;
        [SerializeField] private Image backImage;
        public Image BackImage => backImage;

        [Header("Stats")]
        [SerializeField] private Vector2 basePos;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;
        [SerializeField] private ToastType toastType = ToastType.FLASH;
        [SerializeField] private bool isChangeColor = false;
        [SerializeField] private bool isCustomUpDistance = false;
        [SerializeField] private float customUpDistance = 18f;
        [SerializeField] private bool isCustomHoldDuration = false;
        [SerializeField] private float customHoldDuration = 0.5f;

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

        #endregion

        #region Private Fields

        private Canvas _defaultCanvas;
        private RectTransform _defaultCanvasRect;
        private Canvas _parentCanvas;
        private RectTransform _canvasRect;
        private Camera _mainCamera;
        private Sequence _seq;
        private Vector2 _basePos;
        private Color _defaultBackColor;
        private float _targetScale = 1f;

        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.5f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.35f;
        private readonly float _upDistance = 18f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            if (!imageRectTransform) imageRectTransform = GetComponent<RectTransform>();
            if (!image) image = GetComponentInChildren<Image>();
            if (backImage) _defaultBackColor = backImage.color;

            _defaultCanvas = GetComponentInParent<Canvas>();
            if (_defaultCanvas) _defaultCanvasRect = _defaultCanvas.GetComponent<RectTransform>();
            _parentCanvas = _defaultCanvas;
            _canvasRect = _defaultCanvasRect;
            _mainCamera = Camera.main;

            _basePos = imageRectTransform.anchoredPosition;
            basePos = _basePos;
            SetAlpha(0f);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        private void Reset()
        {
            imageRectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ToastImageArgs toastImageArgs)
            {
                currentArgs = toastImageArgs;
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_Toast_Image");
            }

            gameObject.SetActive(true);
            HandleSpamClick();

            SetValues();
            SetPosition();
            PlayAnim();
        }

        public void HandleSpamClick()
        {
            KillTweens();
            KillAutoReturn();
            _seq?.Kill(false);
            _seq = null;
            imageRectTransform.DOKill(false);
            image.DOKill(false);
            if (backImage) backImage.DOKill(false);
        }

        protected override void SetValues()
        {
            if (currentArgs.customParent != null)
            {
                transform.parent = currentArgs.customParent.transform;
                _parentCanvas = GetComponentInParent<Canvas>();
                if (_parentCanvas) _canvasRect = _parentCanvas.GetComponent<RectTransform>();
            }

            image.sprite = currentArgs.sprite;
            image.SetNativeSize();

            _targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            imageRectTransform.localScale = Vector3.one * _targetScale;

            Color tempColor = currentArgs.customFilterColor != default ? currentArgs.customFilterColor : Color.white;
            tempColor.a = 1f;
            image.color = tempColor;

            if (isChangeColor)
            {
                SetRandomColor();
            }
            else if (backImage)
            {
                backImage.color = _defaultBackColor;
            }
        }

        protected override void ResetValues()
        {
            _seq?.Kill(false);
            _seq = null;
            imageRectTransform.DOKill(false);
            image.DOKill(false);
            if (backImage) backImage.DOKill(false);

            gameObject.SetActive(false);
            imageRectTransform.anchoredPosition = _basePos;
            imageRectTransform.localScale = Vector3.one;
            SetAlpha(0f);

            _parentCanvas = _defaultCanvas;
            _canvasRect = _defaultCanvasRect;
        }

        #endregion

        #region Private Methods

        private void SetPosition()
        {
            if (currentArgs.useScreenPercentage && _canvasRect != null)
            {
                float targetX = _canvasRect.rect.width * (currentArgs.screenPercentage.x / 100f);
                float targetY = _canvasRect.rect.height * (currentArgs.screenPercentage.y / 100f);

                float finalX = targetX - (_canvasRect.rect.width * 0.5f);
                float finalY = targetY - (_canvasRect.rect.height * 0.5f);

                imageRectTransform.anchoredPosition = new Vector2(finalX, finalY);
            }
            else if (currentArgs.isUseAnchoredPos)
            {
                imageRectTransform.anchoredPosition = currentArgs.anchoredPos;
            }
            else if (currentArgs.customPosition != default)
            {
                var cam = _mainCamera ? _mainCamera : (_mainCamera = Camera.main);
                if (cam)
                {
                    imageRectTransform.position = cam.WorldToScreenPoint(currentArgs.customPosition);
                }
                else
                {
                    imageRectTransform.anchoredPosition = _basePos;
                }
            }
            else
            {
                imageRectTransform.anchoredPosition = _basePos;
            }
        }

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
            imageRectTransform.localScale = Vector3.zero;
            SetAlpha(1f);

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(imageRectTransform.DOScale(1.1f * _targetScale, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(imageRectTransform.DOScale(1f * _targetScale, 0.3f * _inDuration).SetEase(_inEase));

            if (holdTime > 0f) _seq.AppendInterval(holdTime);

            _seq.Append(imageRectTransform.DOAnchorPosY(imageRectTransform.anchoredPosition.y + upDist, _upDuration).SetEase(_upEase));
            _seq.Append(imageRectTransform.DOScale(1.1f * _targetScale, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(image.DOFade(0f, 0.7f * _downFadeDuration));
            if (backImage) _seq.Join(backImage.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(imageRectTransform.DOScale(0f, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatAnim(float holdTime, float upDist)
        {
            imageRectTransform.localScale = Vector3.zero;
            SetAlpha(0f);

            float totalDuration = _inDuration + holdTime + _downFadeDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(imageRectTransform.DOAnchorPosY(imageRectTransform.anchoredPosition.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, image.DOFade(1f, _inDuration).SetEase(_inEase));
            if (backImage) _seq.Insert(0f, backImage.DOFade(1f, _inDuration).SetEase(_inEase));
            _seq.Insert(0f, imageRectTransform.DOScale(Vector3.one * _targetScale, _inDuration).SetEase(_inEase));
            _seq.Insert(_inDuration + holdTime, image.DOFade(0f, _downFadeDuration).SetEase(_downEase));
            if (backImage) _seq.Insert(_inDuration + holdTime, backImage.DOFade(0f, _downFadeDuration).SetEase(_downEase));
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

        private void SetAlpha(float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;

            if (backImage)
            {
                Color bc = backImage.color;
                bc.a = alpha;
                backImage.color = bc;
            }
        }

        private void SetRandomColor()
        {
            var colorPairs = ColorHelper.RandomContrastColorPair();
            if (backImage)
            {
                backImage.color = colorPairs.Key;
            }
            else
            {
                image.color = colorPairs.Key;
            }
        }

        #endregion
    }
}