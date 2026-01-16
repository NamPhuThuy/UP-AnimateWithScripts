using System;
using System.Collections;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_ParticleSystem : AnimationBase
    {
        [Header("Components")]
        [SerializeField] private ParticleSystem particleSystem;

        [Header("Custom Material")]
        [Tooltip("If set, overrides the material on all ParticleSystemRenderers under this VFX instance.")]
        [SerializeField] private Material customMaterial;
        [SerializeField] private ParticleSystemRenderer[] PSRenderers;
        private MaterialPropertyBlock _materialPropertyBlock;

        [Header("Persistent Screen Size (Orthographic)")]
        [SerializeField] private bool keepConstantScreenSize = true;

        [Tooltip("If empty, uses args.worldCamera, then Camera.main.")]
        [SerializeField] private Camera referenceCamera;

        [Tooltip("Captured at Play() to compute inverse scaling when the ortho size changes.")]
        [SerializeField] private bool captureBaselineOnPlay = true;

        [SerializeField] private ParticleSystemArgs currentArgs;
        private Coroutine _autoReleaseRoutine;

        private Vector3 _baseLocalScale = Vector3.one;
        private float _baselineOrthoSize = 0f;

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            _baseLocalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_baseLocalScale == Vector3.zero)
                _baseLocalScale = Vector3.one;
        }

        private void OnDisable()
        {
            Destroy(particleSystem.gameObject);
        }


        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is not ParticleSystemArgs a)
                throw new ArgumentException("Invalid argument type for AnimationSimpleParticle");

            currentArgs = a;

            gameObject.SetActive(true);
            isPlaying = true;
            KillTweens();

            if (currentArgs.customParent != null)
                transform.SetParent(currentArgs.customParent, true);

            SetValues();

            // ApplyCustomMaterialIfAny();
            PlayParticle();
        }

        protected override void SetValues()
        {
           

            ParticleSystem ps = Instantiate(currentArgs.particleSystem).GetComponent<ParticleSystem>();
            particleSystem = ps;
            
            customMaterial = currentArgs.customMaterial;
            
            
            if (currentArgs.fromWorld)
            {
                particleSystem.transform.position = new Vector3(currentArgs.worldPosition.x, currentArgs.worldPosition.y, -12f);
            }
            else
            {
                particleSystem.transform.position = new Vector3(
                    currentArgs.anchoredPos.x,
                    currentArgs.anchoredPos.y,
                    particleSystem.transform.position.z
                );
            }
            
            // Custom Values
            if (currentArgs.customTexture != null)
            {
                PSRenderers =  PSRenderers = particleSystem.GetComponentsInChildren<ParticleSystemRenderer>(true);
                DebugLogger.Log(message:$"PSRenderer: {PSRenderers.Length}");
                
                _materialPropertyBlock = new MaterialPropertyBlock();
                ApplyCustomTexture(currentArgs.customTexture);
            }
        }

        protected override void ResetValues()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private void ApplyCustomTexture(Texture texture)
        {
            foreach (var particleSystemRenderer in PSRenderers)
            {
                particleSystemRenderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetTexture("_MainTex", texture);
                particleSystemRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }

        #endregion
       

        private void ApplyCustomMaterialIfAny()
        {
            if (!customMaterial) return;

            var renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                // Use sharedMaterial to avoid per-instance material instantiation.
                renderers[i].material = customMaterial;
                renderers[i].material.color = currentArgs.customColor;
            }
        }

        private void PlayParticle()
        {
            if (!particleSystem)
            {
                StartAutoReturn(0.1f);
                currentArgs.OnComplete?.Invoke();
                return;
            }

            particleSystem.gameObject.SetActive(true);
            ApplyTimeScaleMode(currentArgs.ignoreTimeScale);

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
                t += currentArgs.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            currentArgs.OnComplete?.Invoke();
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
            if (currentArgs.worldCamera) return currentArgs.worldCamera;
            return Camera.main;
        }
    }
}
