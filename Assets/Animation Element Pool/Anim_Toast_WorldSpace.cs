using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    [DefaultExecutionOrder(200)]
    public class Anim_Toast_WorldSpace : AnimationBase
    {
        [Header("Stats")]
        [SerializeField] private ToastWorldSpaceArgs currentArgs;

        [Header("Components")]
        private Transform _transform;
        [SerializeField] private TextMeshPro messageText;
        public TextMeshPro MessageText => messageText;
        [SerializeField] private SpriteRenderer backSprite;
        public SpriteRenderer BackSprite => backSprite;

        private readonly float _inDuration = 0.35f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.5f;
        private readonly float _upDistance = 1.0f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;
        [SerializeField] private ToastType toastType = ToastType.FLASH;
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private bool isChangeColor = false;
        [SerializeField] private bool isCustomUpDistance = false;
        [SerializeField] private float customUpDistance = 1.0f;
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

        private Camera _mainCamera;
        private Sequence _seq;
        private Vector3 _baseWorldPos;
        private Color _defaultBackColor;
        private float _targetScale = 1f;
        private readonly string _fallbackText = "Readying!";

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _transform = transform;
            _mainCamera = Camera.main;
            if (backSprite) _defaultBackColor = backSprite.color;
            _baseWorldPos = _transform.position;
            SetAlpha(0f);
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (faceCamera)
            {
                UpdateCameraRotation();
            }
        }

        void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        void Reset()
        {
            _transform = transform;
            messageText = GetComponentInChildren<TextMeshPro>();
            backSprite = GetComponentInChildren<SpriteRenderer>();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ToastWorldSpaceArgs worldArgs)
            {
                currentArgs = worldArgs;
            }
            else if (args is ToastArgs toastArgs)
            {
                currentArgs = new ToastWorldSpaceArgs
                {
                    OnComplete = toastArgs.OnComplete,
                    message = toastArgs.message,
                    textColor = toastArgs.textColor,
                    textFont = toastArgs.textFont,
                    toastType = toastArgs.toastType,
                    customDuration = toastArgs.customDuration,
                    customScale = toastArgs.customScale,
                    isChangeColor = toastArgs.isChangeColor,
                    isCustomUpDistance = toastArgs.isCustomUpDistance,
                    customUpDistance = toastArgs.customUpDistance,
                    isCustomHoldDuration = toastArgs.isCustomHoldDuration,
                    customHoldDuration = toastArgs.customHoldDuration,
                    worldPosition = toastArgs.customAnchoredPos,
                    targetTransform = toastArgs.customParent
                };
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_Toast_WorldSpace");
            }

            gameObject.SetActive(true);
            KillTweens();
            _seq?.Kill(false);
            messageText.DOKill(false);
            _transform.DOKill(false);
            if (backSprite) backSprite.DOKill(false);

            SetValues();
            PlayAnim();
        }

        protected override void SetValues()
        {
            if (currentArgs.textFont != null)
            {
                messageText.font = currentArgs.textFont;
            }

            if (currentArgs.textSize > 0f)
            {
                messageText.fontSize = currentArgs.textSize;
            }

            SetPosition();

            _targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;

            SetContent(currentArgs.message);

            if (isChangeColor || currentArgs.isChangeColor)
            {
                SetRandomColor();
            }
            else if (backSprite)
            {
                backSprite.color = _defaultBackColor;
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
            _seq?.Kill(false);
            _seq = null;
            messageText.DOKill(false);
            _transform.DOKill(false);
            if (backSprite) backSprite.DOKill(false);

            gameObject.SetActive(false);
            _transform.position = _baseWorldPos;
            _transform.localScale = Vector3.one;
            SetAlpha(0f);
        }

        #endregion

        #region Private Methods

        private void SetPosition()
        {
            Vector3 pos = currentArgs.worldPosition;
            if (currentArgs.targetTransform != null)
            {
                pos = currentArgs.targetTransform.position + currentArgs.worldOffset;
            }
            else
            {
                pos += currentArgs.worldOffset;
            }

            _baseWorldPos = pos;
            _transform.position = pos;

            if (faceCamera)
            {
                UpdateCameraRotation();
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
            _transform.localScale = Vector3.zero;
            SetAlpha(1f);

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_transform.DOScale(1.1f * _targetScale, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(_transform.DOScale(1f * _targetScale, 0.3f * _inDuration).SetEase(_inEase));

            if (holdTime > 0f) _seq.AppendInterval(holdTime);

            _seq.Append(_transform.DOMoveY(_baseWorldPos.y + upDist, _upDuration).SetEase(_upEase));
            _seq.Append(_transform.DOScale(1.1f * _targetScale, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(messageText.DOFade(0f, 0.7f * _downFadeDuration));
            if (backSprite) _seq.Join(backSprite.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_transform.DOScale(0f, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(OnAnimationComplete);
        }

        private void PlayFloatAnim(float holdTime, float upDist)
        {
            _transform.localScale = Vector3.zero;
            SetAlpha(0f);

            float totalDuration = _inDuration + holdTime + _downFadeDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_transform.DOMoveY(_baseWorldPos.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, messageText.DOFade(1f, _inDuration).SetEase(_inEase));
            if (backSprite) _seq.Insert(0f, backSprite.DOFade(1f, _inDuration).SetEase(_inEase));
            _seq.Insert(0f, _transform.DOScale(Vector3.one * _targetScale, _inDuration).SetEase(_inEase));
            _seq.Insert(_inDuration + holdTime, messageText.DOFade(0f, _downFadeDuration).SetEase(_downEase));
            if (backSprite) _seq.Insert(_inDuration + holdTime, backSprite.DOFade(0f, _downFadeDuration).SetEase(_downEase));
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
            Color c = messageText.color;
            c.a = alpha;
            messageText.color = c;

            if (backSprite)
            {
                Color bc = backSprite.color;
                bc.a = alpha;
                backSprite.color = bc;
            }
        }

        private void UpdateCameraRotation()
        {
            Camera cam = currentArgs.customCamera ? currentArgs.customCamera : GetCamera();
            if (cam)
            {
                _transform.rotation = cam.transform.rotation;
            }
        }

        private Camera GetCamera()
        {
            if (!_mainCamera || !_mainCamera.gameObject.activeInHierarchy)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }

        #endregion

        #region Public Setup Methods

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
            if (backSprite)
            {
                backSprite.color = colorPairs.Key;
            }
            else
            {
                messageText.color = colorPairs.Key;
            }
        }

        #endregion
    }
}
