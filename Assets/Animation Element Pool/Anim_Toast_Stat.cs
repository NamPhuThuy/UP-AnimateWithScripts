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
        [SerializeField] private bool isCustomUpDistance = false;
        [SerializeField] private float customUpDistance = 24f;
        [SerializeField] private bool isCustomHoldDuration = false;
        [SerializeField] private float customHoldDuration = 0.8f;

        private Canvas _defaultCanvas;
        private RectTransform _defaultCanvasRect;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRect;
        private Sequence _seq;
        private Vector2 _basePos;
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
            _basePos = _rectTransform.anchoredPosition;
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
                    customAnchoredPos = toastArgs.customAnchoredPos,
                    customScale = toastArgs.customScale,
                    customEnableBackImage = toastArgs.customEnableBackImage,
                    customUpDistance = toastArgs.customUpDistance,
                    customHoldDuration = toastArgs.customHoldDuration,
                    useScreenPercentage = toastArgs.useScreenPercentage,
                    screenPercentage = toastArgs.screenPercentage,
                    OnComplete = toastArgs.OnComplete
                };
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_Toast_Stat");
            }

            gameObject.SetActive(true);
            KillTweens();
            SetValues();
            PlayAnim();
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

            if (currentArgs.useScreenPercentage)
            {
                float targetX = _parentCanvasRect.rect.width * (currentArgs.screenPercentage.x / 100f);
                float targetY = _parentCanvasRect.rect.height * (currentArgs.screenPercentage.y / 100f);

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
                statText.rectTransform.localScale = Vector3.one * currentArgs.customScale;
                if (imgIcon != null) imgIcon.rectTransform.localScale = Vector3.one * currentArgs.customScale;
            }
            else
            {
                backImage.rectTransform.localScale = Vector3.one;
                statText.rectTransform.localScale = Vector3.one;
                if (imgIcon != null) imgIcon.rectTransform.localScale = Vector3.one;
            }

            // Set custom icon
            if (currentArgs.iconSprite != null)
            {
                imgIcon.gameObject.SetActive(true);
                imgIcon.sprite = currentArgs.iconSprite;
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
            _rectTransform.anchoredPosition = _basePos;
            _canvasGroup.alpha = 0f;
            _parentCanvas = _defaultCanvas;
            _parentCanvasRect = _defaultCanvasRect;
        }

        #endregion

        #region Animation Methods

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
            _canvasGroup.alpha = 0f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Join(_canvasGroup.DOFade(1f, 0.7f * _inDuration).SetEase(_inEase));
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
            _rectTransform.localScale = Vector3.one;
            _canvasGroup.alpha = 0f;

            float totalDuration = _upDuration + holdTime + _downFadeDuration;
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + upDist, totalDuration).SetEase(_upEase));
            _seq.Insert(0f, _canvasGroup.DOFade(1f, _upDuration).SetEase(_inEase));
            _seq.Insert(_upDuration + holdTime, _canvasGroup.DOFade(0f, _downFadeDuration).SetEase(_downEase));
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
