// Assets/_Project/UPack-Animate-with-Scripts/Assets/Animation Element Pool/AnimationSimpleParticle.cs
using System;
using System.Collections;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class AnimationSimpleParticle : AnimationBase
    {
        [Header("Components")]
        [SerializeField] private ParticleSystem particleSystem;

        [Header("Custom Material")]
        [Tooltip("If set, overrides the material on all ParticleSystemRenderers under this VFX instance.")]
        [SerializeField] private Material customMaterial;

        [Header("Persistent Screen Size (Orthographic)")]
        [SerializeField] private bool keepConstantScreenSize = true;

        [Tooltip("If empty, uses args.worldCamera, then Camera.main.")]
        [SerializeField] private Camera referenceCamera;

        [Tooltip("Captured at Play() to compute inverse scaling when the ortho size changes.")]
        [SerializeField] private bool captureBaselineOnPlay = true;

        [SerializeField] private SimpleParticleArgs _currentArgs;
        private Coroutine _autoReleaseRoutine;

        private Vector3 _baseLocalScale = Vector3.one;
        private float _baselineOrthoSize = 0f;

        private void Awake()
        {
            if (!particleSystem)
                particleSystem = GetComponentInChildren<ParticleSystem>(true);

            _baseLocalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_baseLocalScale == Vector3.zero)
                _baseLocalScale = Vector3.one;
        }

        public override void Play<T>(T args)
        {
            if (args is not SimpleParticleArgs a)
                throw new ArgumentException("Invalid argument type for AnimationSimpleParticle");

            _currentArgs = a;

            gameObject.SetActive(true);
            isPlaying = true;
            KillTweens();

            if (_currentArgs.customParent != null)
                transform.SetParent(_currentArgs.customParent, true);

            SetValues();

            ApplyCustomMaterialIfAny();
            PlayParticle();
        }

        protected override void SetValues()
        {
            if (_currentArgs.fromWorld)
            {
                transform.position = new Vector3(_currentArgs.worldPosition.x, _currentArgs.worldPosition.y, -12f);
            }
            else
            {
                transform.position = new Vector3(
                    _currentArgs.anchoredPosition.x,
                    _currentArgs.anchoredPosition.y,
                    transform.position.z
                );
            }

            customMaterial = _currentArgs.customMaterial;
        }

        private void ApplyCustomMaterialIfAny()
        {
            if (!customMaterial) return;

            var renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                // Use sharedMaterial to avoid per-instance material instantiation.
                renderers[i].material = customMaterial;
                renderers[i].material.color = _currentArgs.customColor;
            }
        }

        private void PlayParticle()
        {
            if (!particleSystem)
            {
                StartAutoReturn(0.1f);
                _currentArgs.OnComplete?.Invoke();
                return;
            }

            particleSystem.gameObject.SetActive(true);
            ApplyTimeScaleMode(_currentArgs.ignoreTimeScale);

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(true);

            float life = GetEstimatedDuration(particleSystem);

            if (_autoReleaseRoutine != null)
                StopCoroutine(_autoReleaseRoutine);

            _autoReleaseRoutine = StartCoroutine(AutoReleaseAfter(life));
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

                float life = main.startLifetime.mode switch
                {
                    ParticleSystemCurveMode.Constant => main.startLifetime.constant,
                    ParticleSystemCurveMode.TwoConstants => main.startLifetime.constantMax,
                    _ => main.startLifetime.constantMax
                };

                max = Mathf.Max(max, d + life);
            }

            return Mathf.Max(0.1f, max);
        }

        private void ApplyTimeScaleMode(bool ignore)
        {
            if (!particleSystem) return;

            foreach (var p in particleSystem.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = p.main;
                main.useUnscaledTime = ignore;
            }
        }

        public void StopImmediate()
        {
            if (_autoReleaseRoutine != null)
            {
                StopCoroutine(_autoReleaseRoutine);
                _autoReleaseRoutine = null;
            }

            if (particleSystem)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.gameObject.SetActive(false);
            }

            transform.localScale = _baseLocalScale;

            base.EndFast();
        }

        private Camera GetCamera()
        {
            if (referenceCamera) return referenceCamera;
            if (_currentArgs.worldCamera) return _currentArgs.worldCamera;
            return Camera.main;
        }
    }
}
