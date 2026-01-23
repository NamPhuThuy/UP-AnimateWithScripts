using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace NamPhuThuy.AnimateWithScripts
{
    public class ObjScaleAuto : ObjActiveAuto
    {
        [Header("Stats")] 
        [SerializeField] private Vector3 originalScale = Vector3.one;
        [SerializeField] private Vector3 targetScale = Vector3.one * 0.9f;
        
        [SerializeField] private float scaleMultiplier = 0.9f;
        
        [SerializeField] private float time = 0.5f;
        [SerializeField] private float time2 = 0.5f;

        [Header("Behavior")] 
        private Sequence currentSequence;
        
        [Header("Delay")]
        [SerializeField] private float delayBeforeStart = 0f;
        [SerializeField] private float delayBetweenLoops = 0f;

        public override MotionType Type { get => MotionType.SCALE; }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        #region MonoBehaviour

        public override void Play()
        {
            currentSequence = DOTween.Sequence();
            currentSequence.AppendInterval(delayBeforeStart);
            currentSequence.AppendCallback(() =>
            {
                CreateLoopSequence();
            });
        }

        public override void Stop()
        {
            currentSequence?.Kill();
            transform.localScale = originalScale;
        }

        private void OnValidate()
        {
            originalScale = transform.localScale;
            targetScale = originalScale * scaleMultiplier;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        
        private void CreateLoopSequence()
        {
            currentSequence?.Kill();
            
            currentSequence = DOTween.Sequence();
            currentSequence.Append(transform.DOScale(targetScale, time).SetEase(Ease.InOutSine));
            currentSequence.Append(transform.DOScale(originalScale, time2).SetEase(Ease.InOutSine));
            currentSequence.AppendInterval(delayBetweenLoops);
            currentSequence.SetLoops(-1).SetUpdate(true);
        }

        #endregion
    }
}