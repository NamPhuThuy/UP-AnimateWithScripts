/*
Github: https://github.com/NamPhuThuy
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
        
        #endregion

        #region Private Methods
        
        protected void KillTweens()
        {
            for (int i = 0; i < tweens.Count; i++) tweens[i]?.Kill();
            tweens.Clear();
        }
        
        private IEnumerator AutoReturnAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            AnimationManager.Ins.Release(this);
        }
        
        #endregion

        #region Abstract Methods

        // Generic play method that each VFX implements
        public abstract void Play<T>(T args) where T : struct, IAnimationArgs;

        protected abstract void SetValues();
        protected abstract void ResetValues();

        #endregion

        #region Public Methods
        protected Coroutine _autoReturnCoroutine;
        
        public virtual void Recycle()
        {
            isPlaying = false;
            KillTweens();

            AnimationManager.Ins.Release(this);
        }
        
        public virtual void EndFast()
        {
            isPlaying = false;
            KillTweens();
            
            if (_autoReturnCoroutine != null)
            {
                StopCoroutine(_autoReturnCoroutine);
                _autoReturnCoroutine = null;
            }
            AnimationManager.Ins.Release(this);
        }
        
        #endregion

        #region Protected Methods

        protected void StartAutoReturn(float duration)
        {
            if (_autoReturnCoroutine != null) StopCoroutine(_autoReturnCoroutine);
            _autoReturnCoroutine = StartCoroutine(AutoReturnAfter(duration));
        }

        #endregion
    }
}