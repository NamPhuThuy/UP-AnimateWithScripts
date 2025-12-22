using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    
    public class AnimationSimpleParticle : AnimationBase
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private ParticleSystem particleSystem;

        private Canvas _canvas;
        private RectTransform _rectTransform;
        private SimpleParticleArgs _currentArgs;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!particleSystem) particleSystem = GetComponentInChildren<ParticleSystem>(true);
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        void OnDisable()
        {
            StopImmediate();
        }

        #endregion

        #region Private Methods
        
        private void PlayParticle()
        {
            if (!particleSystem)
            {
                StartAutoReturn(0.1f);
                _currentArgs.OnComplete?.Invoke();
                return;
            }

            particleSystem.gameObject.SetActive(true);

            timeScaleModeApply(_currentArgs.ignoreTimeScale);

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(true);

            float life = GetEstimatedDuration(particleSystem);
            StartCoroutine(AutoReleaseAfter(life));
        }

        private IEnumerator AutoReleaseAfter(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += _currentArgs.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            _currentArgs.OnComplete?.Invoke();
            StartAutoReturn(0.05f);
        }
        
        private static float GetEstimatedDuration(ParticleSystem ps)
        {
            float max = 0f;
            foreach (var p in ps.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = p.main;
                float d = main.duration;

                if (main.loop)
                    d = Mathf.Max(d, 0.25f);

                float life = 0f;
                switch (main.startLifetime.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        life = main.startLifetime.constant;
                        break;
                    case ParticleSystemCurveMode.TwoConstants:
                        life = main.startLifetime.constantMax;
                        break;
                    default:
                        life = main.startLifetime.constantMax;
                        break;
                }

                max = Mathf.Max(max, d + life);
            }
            return Mathf.Max(0.1f, max);
        }

        private void timeScaleModeApply(bool ignore)
        {
            if (!particleSystem) return;

            foreach (var p in particleSystem.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = p.main;
                main.useUnscaledTime = ignore;
            }
        }

        #endregion

        #region Override Methods
        
        public override void Play<T>(T args)
        {
            if (args is not SimpleParticleArgs a)
            {
                throw new ArgumentException("Invalid argument type for AnimationSimpleParticle");
            }

            _currentArgs = a;

            gameObject.SetActive(true);
            KillTweens();

            if (_currentArgs.customParent != null)
                transform.SetParent(_currentArgs.customParent, false);
            else if (_canvas != null)
                transform.SetParent(_canvas.transform, false);

            SetValues();
            PlayParticle();
        }

        protected override void SetValues()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            if (_canvas == null || _rectTransform == null)
                return;

            if (_currentArgs.FromWorld)
            {
                var cam = _currentArgs.worldCamera ? _currentArgs.worldCamera : Camera.main;
                if (!cam) return;

                Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, _currentArgs.worldPosition);

                RectTransform canvasRect = _canvas.transform as RectTransform;
                if (!canvasRect) return;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screen,
                        _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                        out var local))
                {
                    _rectTransform.anchoredPosition = local;
                }
            }
            else
            {
                _rectTransform.anchoredPosition = _currentArgs.anchoredPosition;
            }
        }
        
        public void StopImmediate()
        {
            if (particleSystem)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.gameObject.SetActive(false);
            }
            base.EndFast();
        }
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            if (!particleSystem)
            {
                particleSystem = GetComponentInChildren<ParticleSystem>(true);
            }

            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        #endregion

        
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(AnimationSimpleParticle))]
    [CanEditMultipleObjects]
    public class AnimationSimpleParticleEditor : Editor
    {
        private AnimationSimpleParticle script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (AnimationSimpleParticle)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}