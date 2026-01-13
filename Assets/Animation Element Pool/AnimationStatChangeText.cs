using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class AnimationStatChangeText : AnimationBase
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI animText;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Vector3 targetPosition;

        [Header("Stats")]
        [SerializeField] private Vector2 moveDistance;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float textSizeMul = 1.3f;
        [SerializeField]  private StatChangeTextArgs currentArgs;

        private Transform _initialParent;

        void Awake()
        {
            _initialParent = transform.parent;
        }

        #region Override Methods
        
        public override void Play<T>(T args)
        {
            if (args is StatChangeTextArgs statArgs)
            {
                currentArgs = statArgs;
                SetValues();
                PlayStatChangeText();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXStatChangeText");
            }
        }
        
        protected override void SetValues()
        {
            if (currentArgs.Duration > 0f)
            {
                duration = currentArgs.Duration;
            }
            
            moveDistance = currentArgs.MoveDistance;
            targetObject = currentArgs.TargetObject;

            targetText = targetObject.GetComponent<TextMeshProUGUI>();
            if (targetText != null)
            {
                animText.CopyProperties(targetText);
                
                var targetRect = targetText.GetComponent<RectTransform>();
                var vfxRect = GetComponent<RectTransform>();
            
            
                vfxRect.sizeDelta = targetRect.sizeDelta;
                vfxRect.anchorMin = targetRect.anchorMin;
                vfxRect.anchorMax = targetRect.anchorMax;
                vfxRect.pivot = targetRect.pivot;
            }
            
            _initialParent = currentArgs.TargetObject.transform;
            
            // Set Text Properties
            
            animText.fontSize *= textSizeMul;
            animText.enableAutoSizing = false;
           
            // Set Text Color
            if (currentArgs.Amount >= 0)
            {
                animText.text = $"+{currentArgs.Amount}{currentArgs.AdditionalIconText}";
                animText.color = Color.green;
            }
            else
            {
                animText.text = $"-{currentArgs.Amount}{currentArgs.AdditionalIconText}";
                animText.color = Color.red;
            }

            if (currentArgs.IsBold)
            {   
                animText.fontStyle |= FontStyles.Bold;
            }

            if (currentArgs.CustomColor != default)
            {
                animText.color = currentArgs.CustomColor;
            }
        }

        protected override void ResetValues()
        {
            animText.fontStyle &= ~FontStyles.Bold;
        }

        #endregion

        private Vector3 GetRandomPos(float range)
        {
            return new Vector3(
                UnityEngine.Random.Range(-range, range),
                UnityEngine.Random.Range(-range, range),
                0f
            );
        }

        private Vector2 startPosition;
        private Vector2 endPosition;
        private void PlayStatChangeText()
        {
            // transform.SetParent(targetObject.transform.parent, true);

            
            // Convert targetObject's world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentArgs.TargetObject.transform.position);
            Debug.Log(message:$"screenPos: {screenPos}");
            startPosition = new Vector2(screenPos.x, screenPos.y) + currentArgs.RectTransformOffset;
            endPosition = startPosition + currentArgs.MoveDistance;
            
            Debug.Log(message:$"startPosi: {startPosition}, endPosi: {endPosition}");
            
            // Set initial anchored position
            RectTransform rt = GetComponent<RectTransform>();
            rt.position = startPosition;
            
            Vector2 offset = Vector2Helper.RandomPointInRange(35f, preferHorizontal:true);
            
            rt.position += (Vector3)offset/*currentArgs.RectTransformOffset*/;
            
            animText.DOFade(1f, 0f);
            gameObject.SetActive(true);

            Vector3 moveTarget = transform.position + (Vector3)moveDistance;
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(moveTarget, duration));
            seq.Join(animText.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                // transform.SetParent(_initialParent, true);
                currentArgs.OnComplete?.Invoke();
                ResetValues();
                Recycle();
            });
        }
    }
}
