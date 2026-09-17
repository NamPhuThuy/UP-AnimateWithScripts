using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// World-space variant of Anim_ItemFly.
    /// Spawns SpriteRenderer items that scatter, then fly along 3D Bezier arcs to a target.
    /// Supports camera billboard, target punch, and per-item arrival callbacks.
    /// </summary>
    public class Anim_ItemFly_WS : AnimationBase
    {
        #region Constants

        private const int DEFAULT_POOL_SIZE = 8;
        private const int CURVE_POINT_COUNT = 5;
        private const float INITIAL_DELAY = 0.2f;
        private const float SCATTER_MIN = 0.8f;
        private const float SCATTER_MAX = 1.2f;
        private const float SCALE_MIN = 0.8f;
        private const float SCALE_MAX = 1.2f;
        private const float PUNCH_SCALE_MULTIPLIER = 0.15f;
        private const float PUNCH_DURATION = 0.25f;
        private const float PUNCH_THROTTLE_INTERVAL = 0.05f;

        #endregion

        #region Serializable Fields

        [Header("Stats")]
        [SerializeField] private float totalVfxDuration = 1.6f;
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private float pathDuration = 0.4f;
        [SerializeField] private float scatterRadius = 1.0f;
        [SerializeField] private float arcHeight = 1.5f;

        [Header("Components")]
        [SerializeField] private GameObject itemPrefab; // must have SpriteRenderer

        [Header("Flags")]
        [SerializeField] private bool faceCamera = true;

        #endregion

        #region Private Fields

        private ItemFlyWSArgs _currentArgs;
        private Camera _mainCamera;
        private Transform _transform;

        private readonly List<Transform> _itemPool = new();
        private readonly List<SpriteRenderer> _itemRenderers = new();
        private readonly List<Vector3[]> _pathBuffers = new();

        private int _activeItemCount;
        private int _remainingItems;
        private float _spawnStepDelay;

        // Target punch management (static to share across instances)
        private static readonly Dictionary<Transform, Vector3> s_OriginalScales = new();
        private static readonly Dictionary<Transform, float> s_LastPunchTime = new();

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            _transform = transform;
            _mainCamera = Camera.main;
            EnsurePool(DEFAULT_POOL_SIZE);
        }

        private void LateUpdate()
        {
            if (!isPlaying) return;

            bool shouldBillboard = _currentArgs.faceCamera || faceCamera;
            if (!shouldBillboard) return;

            Camera cam = _currentArgs.customCamera ? _currentArgs.customCamera : GetCamera();
            if (!cam) return;

            Quaternion camRot = cam.transform.rotation;
            for (int i = 0; i < _activeItemCount; i++)
            {
                if (_itemPool[i].gameObject.activeSelf)
                {
                    _itemPool[i].rotation = camRot;
                }
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ItemFlyWSArgs wsArgs)
            {
                _currentArgs = wsArgs;
                gameObject.SetActive(true);
                isPlaying = true;
                HandleSpamClick();
                SetValues();
                PlayAnim();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for Anim_ItemFly_WS");
            }
        }

        public void HandleSpamClick()
        {
            KillTweens();
            KillAutoReturn();
            for (int i = 0; i < _itemPool.Count; i++)
            {
                _itemPool[i].DOKill(false);
                _itemPool[i].gameObject.SetActive(false);
            }
        }

        protected override void SetValues()
        {
            _activeItemCount = Mathf.Max(1, _currentArgs.itemAmount > 0 ? _currentArgs.itemAmount : DEFAULT_POOL_SIZE);
            _remainingItems = _activeItemCount;

            float totalDur = _currentArgs.totalDuration > 0f ? _currentArgs.totalDuration : totalVfxDuration;
            float flightDur = _currentArgs.pathDuration > 0f ? _currentArgs.pathDuration : pathDuration;

            float spacingBudget = Mathf.Max(0f, totalDur - INITIAL_DELAY - bounceDuration - flightDur);
            _spawnStepDelay = (_activeItemCount > 1) ? spacingBudget / (_activeItemCount - 1) : 0f;

            _transform.position = _currentArgs.startWorldPosition;

            EnsurePool(_activeItemCount);

            float scatter = _currentArgs.scatterRadius > 0f ? _currentArgs.scatterRadius : scatterRadius;
            float baseScale = !Mathf.Approximately(_currentArgs.itemScale, 0f) ? _currentArgs.itemScale : 1f;

            for (int i = 0; i < _activeItemCount; i++)
            {
                var item = _itemPool[i];
                var sr = _itemRenderers[i];

                sr.sprite = _currentArgs.itemSprite;
                sr.color = Color.white;

                // Random scatter offset around start position
                Vector3 offset = new Vector3(
                    Random.Range(-scatter, scatter) * Random.Range(SCATTER_MIN, SCATTER_MAX),
                    Random.Range(-scatter, scatter) * Random.Range(SCATTER_MIN, SCATTER_MAX),
                    Random.Range(-scatter * 0.5f, scatter * 0.5f)
                );
                item.position = _currentArgs.startWorldPosition + offset;
                item.localScale = Vector3.zero;
            }
        }

        protected override void ResetValues()
        {
            isPlaying = false;
            KillTweens();
            for (int i = 0; i < _itemPool.Count; i++)
            {
                _itemPool[i].DOKill(false);
                _itemPool[i].gameObject.SetActive(false);
                _itemPool[i].localScale = Vector3.one;
            }
            gameObject.SetActive(false);
        }

        #endregion

        #region Pool Management

        private void EnsurePool(int required)
        {
            while (_itemPool.Count < required)
            {
                var go = Instantiate(itemPrefab, _transform);
                go.SetActive(false);
                var t = go.transform;
                _itemPool.Add(t);
                _itemRenderers.Add(go.GetComponent<SpriteRenderer>());
                _pathBuffers.Add(new Vector3[CURVE_POINT_COUNT]);
            }
        }

        #endregion

        #region Animation Flow

        private void PlayAnim()
        {
            float baseScale = !Mathf.Approximately(_currentArgs.itemScale, 0f) ? _currentArgs.itemScale : 1f;

            for (int i = 0; i < _activeItemCount; i++)
            {
                SetupSpawnItem(i, baseScale);
            }
        }

        private void SetupSpawnItem(int index, float baseScale)
        {
            var item = _itemPool[index];
            item.gameObject.SetActive(true);

            float randomScale = Random.Range(SCALE_MIN, SCALE_MAX) * baseScale;

            // Pop-in animation
            var seq = DOTween.Sequence();
            seq.Append(item.DOScale(randomScale * 1.2f, 0.3f).SetEase(Ease.InOutSine));
            seq.Append(item.DOScale(randomScale, 0.2f).SetEase(Ease.InOutSine));
            tweens.Add(seq);

            if (index == _activeItemCount - 1)
            {
                seq.OnComplete(OnAllItemsSpawned);
            }
        }

        private void OnAllItemsSpawned()
        {
            float flightDur = _currentArgs.pathDuration > 0f ? _currentArgs.pathDuration : pathDuration;
            float height = _currentArgs.arcHeight > 0f ? _currentArgs.arcHeight : arcHeight;

            for (int i = 0; i < _activeItemCount; i++)
            {
                AnimateFlightItem(i, flightDur, height);
            }
        }

        private void AnimateFlightItem(int index, float flightDur, float height)
        {
            var item = _itemPool[index];
            Vector3 startPos = item.position;
            Vector3 endPos = _currentArgs.targetTransform
                ? _currentArgs.targetTransform.position
                : _currentArgs.targetWorldPosition;

            // Build 3D Bezier path
            var path = _pathBuffers[index];
            FillCurvePath3D(index, startPos, endPos, height, path);

            float delay = INITIAL_DELAY + _spawnStepDelay * index;

            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(item.DOPath(path, flightDur, PathType.CatmullRom).SetEase(Ease.InOutSine));
            seq.OnComplete(() => OnItemArrived(item));
            tweens.Add(seq);
        }

        private void OnItemArrived(Transform item)
        {
            item.gameObject.SetActive(false);
            _remainingItems--;

            // Per-item callback
            try
            {
                _currentArgs.OnItemArrive?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in OnItemArrive callback: {ex.Message}\n{ex.StackTrace}");
            }

            // Punch target
            if (_currentArgs.punchTarget && _currentArgs.targetTransform)
            {
                ApplyPunchEffect(_currentArgs.targetTransform);
            }

            // Last item
            if (_remainingItems <= 0)
            {
                try
                {
                    _currentArgs.OnComplete?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnComplete callback: {ex.Message}\n{ex.StackTrace}");
                }

                ResetValues();
                Recycle();
            }
        }

        #endregion

        #region Curve Generation

        private enum CurveType
        {
            EXPONENTIAL = 0,
            SINE = 1,
            PARABOLIC = 2,
            LINEAR = 3,
            LOGARITHMIC = 4,
            COUNT
        }

        private void FillCurvePath3D(int itemIndex, Vector3 start, Vector3 end, float height, Vector3[] path)
        {
            CurveType curveType = (CurveType)(itemIndex % (int)CurveType.COUNT);
            Vector3 distance = end - start;

            // Perpendicular up direction for the arc
            Vector3 up = Vector3.up;

            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float t = (float)j / (CURVE_POINT_COUNT - 1);
                float yFactor = 0f;

                switch (curveType)
                {
                    case CurveType.EXPONENTIAL:
                        yFactor = EvaluateSaturationCurve(t, 8f);
                        break;
                    case CurveType.SINE:
                        yFactor = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.2f;
                        break;
                    case CurveType.PARABOLIC:
                        yFactor = t * t * 1.5f;
                        break;
                    case CurveType.LINEAR:
                        yFactor = t;
                        break;
                    case CurveType.LOGARITHMIC:
                        yFactor = Mathf.Log10(1 + 9 * t);
                        break;
                }

                float randomOffset = Random.Range(-0.1f, 0.1f);
                yFactor = Mathf.Clamp01(yFactor + randomOffset);

                // Interpolate along the distance vector (X/Z), arc upward (Y)
                Vector3 basePos = start + distance * t;

                // Arc: parabola that peaks at midpoint
                float arcFactor = 4f * t * (1f - t); // peaks at t=0.5 with value 1
                Vector3 arcOffset = up * (height * arcFactor);

                // Add lateral randomness proportional to scatter
                float lateralRandom = Random.Range(-0.15f, 0.15f) * height;
                Vector3 lateralOffset = Vector3.Cross(distance.normalized, up).normalized * lateralRandom;

                path[j] = basePos + arcOffset + lateralOffset;
            }

            // Force last point exactly at target
            path[CURVE_POINT_COUNT - 1] = end;
        }

        private float EvaluateSaturationCurve(float x, float k, float yMax = 1f)
        {
            return yMax * (1f - Mathf.Exp(-k * x));
        }

        #endregion

        #region Punch Effect

        private void ApplyPunchEffect(Transform target)
        {
            float now = Time.time;
            if (s_LastPunchTime.TryGetValue(target, out float lastTime) && (now - lastTime) < PUNCH_THROTTLE_INTERVAL)
            {
                return;
            }
            s_LastPunchTime[target] = now;

            if (!s_OriginalScales.TryGetValue(target, out Vector3 baseScale))
            {
                baseScale = target.localScale;
                s_OriginalScales[target] = baseScale;
            }

            target.DOKill(false);
            target.localScale = baseScale;

            target.DOPunchScale(baseScale * PUNCH_SCALE_MULTIPLIER, PUNCH_DURATION, vibrato: 6, elasticity: 0.5f)
                .SetTarget(target)
                .OnComplete(() =>
                {
                    target.localScale = baseScale;
                    s_OriginalScales.Remove(target);
                    s_LastPunchTime.Remove(target);
                })
                .OnKill(() =>
                {
                    target.localScale = baseScale;
                });
        }

        #endregion

        #region Helpers

        private Camera GetCamera()
        {
            if (!_mainCamera || !_mainCamera.gameObject.activeInHierarchy)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }

        #endregion
    }
}
