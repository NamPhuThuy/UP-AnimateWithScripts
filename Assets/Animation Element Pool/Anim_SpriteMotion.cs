using UnityEngine;
using DG.Tweening;
using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// A reusable component for animating a sprite. Designed to be used with an object pool.
    /// It controls movement, fading, and scaling animations using DOTween.
    /// </summary>
    public class Anim_SpriteMotion : AnimationBase
    {
        #region Private Serializable Fields

        [Header("Flags")] 
        [SerializeField] private ObjActiveAuto.MotionType motionType;

        [Header("Components")] 
        [SerializeField] private Transform interactTransform;// The transform that this script control
        [SerializeField] private ObjClockwiseAnimAuto objClockwiseAnimAuto;
        [SerializeField] private ObjRotateAuto objRotateAuto;
        [SerializeField] private ObjScaleAuto objScaleAuto;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Stats")] 
        [SerializeField] private float duration = 2f;
        [SerializeField] private Vector3 worldSpaceStartPosi;
        [SerializeField] private SpriteMotionArgs currentArgs;
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;

        private Sequence _sequence;
        #endregion

        #region MonoBehaviour Callbacks
        
        
        private void OnDestroy()
        {
            // Final cleanup to prevent any tweens from running after destruction.
            _sequence?.Kill();
        }

        #endregion

        /// <summary>
        /// Plays a simple motion from a start to an end position.
        /// </summary>
        public void PlayMotion()
        {
            interactTransform.position = worldSpaceStartPosi;

            _sequence = DOTween.Sequence();
            _sequence.AppendInterval(duration).OnComplete(EndFast);

            switch (motionType)
            {
                case ObjActiveAuto.MotionType.CLOCK:
                    objClockwiseAnimAuto.Play();
                    
                    break;
                case ObjActiveAuto.MotionType.ROTATE:
                    objRotateAuto.Play();
                    
                    break;
                case ObjActiveAuto.MotionType.SCALE:
                    objScaleAuto.Play();
                    break;
            }
            
        }


        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is SpriteMotionArgs spriteMotionArgs)
            {
                currentArgs = spriteMotionArgs;
                gameObject.SetActive(true);
                SetValues();
                KillTweens();
                PlayMotion();
            }
        }

        protected override void SetValues()
        {
            // Must have values
            if (currentArgs.sprite == null)
            {
                DebugLogger.LogError(message:$"Sprite is missing");
            }
            spriteRenderer.sprite = currentArgs.sprite;

            if (currentArgs.customVerticalSize != 0)
            {
                DebugLogger.Log(message:$"currentArgs.customVerticalSize: {currentArgs.customVerticalSize}, spriteRenderer.sprite.textureRect.height: {spriteRenderer.sprite.textureRect.height}, spriteRenderer.sprite.pixelsPerUnit: {spriteRenderer.sprite.pixelsPerUnit}; res = {currentArgs.customVerticalSize / (spriteRenderer.sprite.textureRect.height / spriteRenderer.sprite.pixelsPerUnit)}");
                float multiply = currentArgs.customVerticalSize / (spriteRenderer.sprite.textureRect.height / spriteRenderer.sprite.pixelsPerUnit);
                transform.localScale = new Vector3(multiply, multiply, multiply);
            }

            motionType = currentArgs.motionType;
            worldSpaceStartPosi = currentArgs.worldSpaceStartPosi;
            
            // Custom values
            if (currentArgs.customDuration > 0f)
            {
                duration = currentArgs.customDuration;
            }

            if (currentArgs.customSortingOrder != 0)
            {
                spriteRenderer.sortingOrder = currentArgs.customSortingOrder;
            }

            if (currentArgs.customSortingLayer != null)
            {
                sortingLayerName = currentArgs.customSortingLayer;
                spriteRenderer.sortingLayerName = currentArgs.customSortingLayer;
            }

        }

        protected override void ResetValues()
        {
            gameObject.SetActive(false);
            _sequence?.Kill();
            transform.localScale = Vector3.one;
            spriteRenderer.color = Color.white;
            
            objClockwiseAnimAuto.Stop();
            objRotateAuto.Stop();
            objScaleAuto.Stop();
        }
        
        public override void EndFast()
        {
            // DebugLogger.Log();
            base.EndFast();
            _sequence?.Kill();
            ResetValues();
        }

        #endregion
    }
}
