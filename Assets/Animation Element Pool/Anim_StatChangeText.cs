using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_StatChangeText : AnimationBase
    {
        [Header("Native Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI nativeText;
        
        [Header("External Components")]
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Vector3 targetPosition;

        [Header("Stats")]
        [SerializeField] private Vector2 moveDistance;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float textSizeMul = 1.3f;
        [SerializeField]  private StatChangeTextArgs currentArgs;

        #region Override Methods
        
        public override void Play<T>(T args)
        {
            if (args is StatChangeTextArgs statArgs)
            {
                currentArgs = statArgs;
                SetValues();
                PlayAnim();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXStatChangeText");
            }
        }
        
        protected override void SetValues()
        {
            if (currentArgs.duration > 0f)
            {
                duration = currentArgs.duration;
            }
            
            moveDistance = currentArgs.moveDistance;
            targetObject = currentArgs.targetObject;

            targetText = targetObject.GetComponent<TextMeshProUGUI>();
            if (targetText != null)
            {
                nativeText.CopyProperties(targetText);
                
                var targetRect = targetText.GetComponent<RectTransform>();
                var vfxRect = GetComponent<RectTransform>();
            
            
                vfxRect.sizeDelta = targetRect.sizeDelta;
                vfxRect.anchorMin = targetRect.anchorMin;
                vfxRect.anchorMax = targetRect.anchorMax;
                vfxRect.pivot = targetRect.pivot;
            }
            
            // Set Text Properties
            
            nativeText.enableAutoSizing = false;
            nativeText.fontSize *= textSizeMul;

            if (currentArgs.isBold)
            {   
                nativeText.fontStyle |= FontStyles.Bold;
            }
        }

        protected override void ResetValues()
        {
            nativeText.fontStyle &= ~FontStyles.Bold;
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
        private void PlayAnim()
        {
            DebugLogger.Log();
            Canvas canvas = GetComponentInParent<Canvas>();
    
            // Convert world position to canvas position
            Vector2 canvasPosition;
            
            if (currentArgs.isUseAnchoredPos)
            {
                // rectTransform.anchoredPosition = currentArgs.anchoredPos;
            }
            else
            {
                rectTransform.position = Camera.main.WorldToScreenPoint(currentArgs.targetObject.transform.position);
            }
            
            DebugLogger.Log(message:$"Canvas Rendermode: {canvas.renderMode}");
           
            nativeText.DOFade(1f, 0f);
            gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Join(nativeText.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                try
                {
                    currentArgs.OnComplete?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnComplete callback: {ex.Message}\n{ex.StackTrace}");
                }
                ResetValues();
                Recycle();
            });
        }
    }
}
