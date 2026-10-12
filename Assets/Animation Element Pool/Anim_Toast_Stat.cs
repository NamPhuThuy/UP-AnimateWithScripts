using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Anim_Toast_Stat : AnimationBase
    {
        [Header("Stats")] 
        [SerializeField] private ToastStatArgs currentArgs;
        
        [Header("Components")]
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private Image imgIcon;
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
        private readonly string _fallbackText = "Stat Up!";

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
            if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
            _defaultCanvas = GetComponentInParent<Canvas>();
            if (_defaultCanvas) _defaultCanvasRect = _defaultCanvas.GetComponent<RectTransform>();
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
            _defaultSizeDelta = _rectTransform.sizeDelta;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ToastStatArgs statArgs)
            {
                currentArgs = statArgs;
            }
            else if (args is ToastArgs toastArgs)
            {
                currentArgs = new ToastStatArgs
                {
                    statValue = toastArgs.message,
                    textFont = toastArgs.textFont,
                    toastType = toastArgs.toastType,
                    customScale = toastArgs.customScale,
                    customEnableBackImage = toastArgs.customEnableBackImage,
                    customUpDistance = toastArgs.customUpDistance,
                    customHoldDuration = toastArgs.customHoldDuration,
                    useScreenPercentage = toastArgs.useScreenPercentage,
                    screenPercentage = toastArgs.screenPercentage,
                    backgroundSizePercent = toastArgs.backgroundSizePercent,
                    OnComplete = toastArgs.OnComplete
                };
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_Toast_Stat");
            }

            gameObject.SetActive(true);
            HandleSpamClick();
            SetValues();
            PlayAnim();
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
                statText.font = currentArgs.textFont;
            }

            if (currentArgs.customParent != null)
            {
                transform.parent = currentArgs.customParent.transform;
                _parentCanvas = GetComponentInParent<Canvas>();
                if (_parentCanvas) _parentCanvasRect = _parentCanvas.GetComponent<RectTransform>();
            }

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
            if (statText.rectTransform != _rectTransform)
            {
                statText.rectTransform.localScale = Vector3.one * targetScale;
            }
            if (imgIcon != null && imgIcon.rectTransform != _rectTransform)
            {
                imgIcon.rectTransform.localScale = Vector3.one * targetScale;
            }

            // Set custom icon
            if (currentArgs.iconSprite != null)
            {
                imgIcon.gameObject.SetActive(true);
                imgIcon.sprite = currentArgs.iconSprite;
            }
            else if (imgIcon != null)
            {
                imgIcon.gameObject.SetActive(false);
            }

            // Set merged stat content
            SetContent(currentArgs.statType, currentArgs.statValue);

            // Set text color
            if (currentArgs.textColor != default)
            {
                statText.color = currentArgs.textColor;
            }
            else
            {
                statText.color = Color.white;
            }
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

        #region Animation Methods

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

            if (currentArgs.customDuration != 0f)
                StartAutoReturn(currentArgs.customDuration);
        }

        private void PlayFlashAnim(float holdTime, float upDist)
        {
            float targetScale = !Mathf.Approximately(currentArgs.customScale, 0f) ? currentArgs.customScale : 1f;
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f * targetScale, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Join(_canvasGroup.DOFade(1f, _inDuration).SetEase(_inEase));
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

        #endregion

        #region Content Setup

        public void SetContent(string statType, string statValue)
        {
            if (string.IsNullOrEmpty(statType))
            {
                statText.text = !string.IsNullOrEmpty(statValue) ? statValue : _fallbackText;
            }
            else if (string.IsNullOrEmpty(statValue))
            {
                statText.text = statType;
            }
            else
            {
                statText.text = $"{statType} {statValue}";
            }
        }

        public void SetContent(string statType, string statValue, Action moreSetup)
        {
            SetContent(statType, statValue);
            moreSetup?.Invoke();
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

        #region Editor Methods

#if UNITY_EDITOR
        public void ResetValuesInEditor()
        {
            Undo.RecordObject(this, "Reset Values");
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string n = img.name.ToLowerInvariant();
                if (img.gameObject == gameObject || n.Contains("back") || n.Contains("bg"))
                {
                    backImage = img;
                }
                else if (n.Contains("icon") || n.Contains("img"))
                {
                    imgIcon = img;
                }
            }

            statText = GetComponentInChildren<TextMeshProUGUI>(true);
            EditorUtility.SetDirty(this);
        }
#endif

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Anim_Toast_Stat))]
    [CanEditMultipleObjects]
    public class Anim_Toast_StatEditor : Editor
    {
        private Anim_Toast_Stat _target;
        
        private void OnEnable()
        {
            _target = (Anim_Toast_Stat)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Reset Values"))
            {
                _target.ResetValuesInEditor();
            }
        }
    }
#endif
}
