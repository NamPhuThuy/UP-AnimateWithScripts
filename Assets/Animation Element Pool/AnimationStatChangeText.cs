using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NamPhuThuy.AnimateWithScripts
{
    public class AnimationStatChangeText : AnimationBase
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI targetText;

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

        private void SetTextColor()
        {
            if (currentArgs.Amount >= 0)
            {
                text.text = $"+{currentArgs.Amount}";
                text.color = Color.green;
            }
            else
            {
                text.text = $"{currentArgs.Amount}";
                text.color = Color.red;
            }

            if (currentArgs.Color != default)
            {
                text.color = currentArgs.Color;
            }
        }

        #region Override Methods
        
        public override void Play<T>(T args)
        {
            if (args is StatChangeTextArgs statArgs)
            {
                currentArgs = statArgs;
                PlayStatChangeText();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXStatChangeText");
            }
        }
        
        protected override void SetValues()
        {
            moveDistance = currentArgs.MoveDistance;
            targetText = currentArgs.InitialParent.GetComponent<TextMeshProUGUI>();
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

        

        private void PlayStatChangeText()
        {
            SetValues();

            var targetRect = targetText.GetComponent<RectTransform>();
            var vfxRect = GetComponent<RectTransform>();

            transform.SetParent(targetText.transform.parent, true);

            vfxRect.sizeDelta = targetRect.sizeDelta;
            vfxRect.anchorMin = targetRect.anchorMin;
            vfxRect.anchorMax = targetRect.anchorMax;
            vfxRect.pivot = targetRect.pivot;

            Vector3 moreOffset = GetRandomPos(0.4f);
            transform.position = targetText.transform.position + (Vector3)currentArgs.Offset + moreOffset;

            text.CopyProperties(targetText);
            text.fontSize *= textSizeMul;
            text.enableAutoSizing = false;

            SetTextColor();

            text.DOFade(1f, 0f);
            gameObject.SetActive(true);

            Vector3 moveTarget = transform.position + (Vector3)moveDistance;
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(moveTarget, duration));
            seq.Join(text.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                transform.SetParent(_initialParent, true);
                currentArgs.OnComplete?.Invoke();
            });
        }
    }
}
