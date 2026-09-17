/*
Github: https://github.com/NamPhuThuy/UP-AnimateWithScripts
*/


using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    
    public abstract class AnimationBase : MonoBehaviour
    {
        #region Private Serializable Fields

        protected readonly List<Tween> tweens = new();
        [SerializeField] protected bool isPlaying;
        
        public bool IsInPool { get; internal set; }

        #endregion

        #region Private Methods
        
        protected void KillTweens()
        {
            for (int i = 0; i < tweens.Count; i++) tweens[i]?.Kill();
            tweens.Clear();
        }

        protected void KillAutoReturn()
        {
            if (_autoReturnTween != null && _autoReturnTween.IsActive())
            {
                _autoReturnTween.Kill();
            }
            _autoReturnTween = null;
        }
        
        #endregion

        #region Abstract Methods

        // Generic play method that each VFX implements
        public abstract void Play<T>(T args) where T : struct, IAnimationArgs;

        protected abstract void SetValues();
        protected abstract void ResetValues();

        #endregion

        #region Public Methods
        protected Tween _autoReturnTween;
        
        public virtual void Recycle()
        {
            isPlaying = false;
            KillAutoReturn();
            KillTweens();

            AnimationManager.Ins.Release(this);
        }
        
        public virtual void EndFast()
        {
            isPlaying = false;
            KillAutoReturn();
            KillTweens();
            
            AnimationManager.Ins.Release(this);
        }
        
        #endregion

        #region Protected Methods

        protected void StartAutoReturn(float duration)
        {
            KillAutoReturn();
            _autoReturnTween = DOVirtual.DelayedCall(duration, () => AnimationManager.Ins.Release(this), ignoreTimeScale: false);
        }

        #endregion
    }
}