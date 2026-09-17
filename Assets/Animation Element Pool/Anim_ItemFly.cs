/*
Github: https://github.com/NamPhuThuy/UP-AnimateWithScripts
*/

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_ItemFly : AnimationBase
    {
        private const int CURVE_POINT_COUNT = 5;
        private const float CURVE_STRENGTH = 8f;
        private const float INITIAL_DELAY = 0.2f;
        private const float BOUNCE_MIN = 100f;
        private const float BOUNCE_MAX = 200f;
        private const float SCALE_MIN = 0.8f;
        private const float SCALE_MAX = 1.2f;
        private const float SIZE_RANDOM_MIN = 1.1f;
        private const float SIZE_RANDOM_MAX = 1.3f;
        private const string NUMBER_FORMAT = "{0}";

        [Header("Stats")]
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private int totalAmount;
        [SerializeField] private int prevValue;
        [Tooltip("Total duration from first spawn until last item lands")]
        [SerializeField] private float totalVfxDuration = 1.6f;
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private float pathDuration = 0.4f;
        
        [SerializeField] private ItemFlyArgs currentArgs;

        [Header("Native Components")]
        [SerializeField] private GameObject itemContainer;
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private List<RectTransform> itemList;

        [Header("External Components")]
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private TextMeshProUGUI realResourceText;
        [SerializeField] private Transform targetInteractTransform;
        
       
        [SerializeField] private RectTransform rippleFxCointainer;
        [SerializeField] private ParticleSystem rippleFx;
        
       

        #region Private Fields

        #region Target Punch Scale Management

        private static readonly Dictionary<Transform, Vector3> s_OriginalScales = new();
        private static readonly Dictionary<Transform, float> s_LastPunchTime = new();
        private const float PUNCH_THROTTLE_INTERVAL = 0.05f;
        private const float PUNCH_SCALE_MULTIPLIER = 0.15f;
        private const float PUNCH_DURATION = 0.25f;

        #endregion

        private readonly int _initialPoolSize = 8;
        private int _activeItemCount;
        private int _unitValue;
        private int _remainingItems;
        private float _spawnStepDelay;
        private readonly List<Vector3[]> _pathBuffers = new();
        private readonly List<Image> _itemImages = new();
        
        private bool IsHaveRealText => realResourceText != null;
        
        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            CreatePool();
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ItemFlyArgs itemFlyArgs)
            {
                currentArgs = itemFlyArgs;
                gameObject.SetActive(true);
                SetValues();
                KillTweens();
                PlayAnim();
            }
        }

        #endregion

        #region Set up

        protected override void SetValues()
        {
            // COMPONENTS
            realResourceText = currentArgs.targetText.GetComponent<TextMeshProUGUI>();
            targetInteractTransform = currentArgs.targetInteractTransform ? currentArgs.targetInteractTransform : null;
            itemSprite = currentArgs.itemSprite ?? itemSprite;
            
            
            targetPosition = currentArgs.targetInteractTransform ? currentArgs.targetInteractTransform.transform.position : currentArgs.targetText.position;
            
            // VALUES
            totalAmount = currentArgs.addValue;
            prevValue = currentArgs.prevValue;

            if (!Mathf.Approximately(currentArgs.delayBetweenItems, 0))
            {
                pathDuration = currentArgs.delayBetweenItems;
            }
            
            _activeItemCount = Mathf.Max(1, currentArgs.itemAmount > 0 ? currentArgs.itemAmount : _initialPoolSize);
            _remainingItems = _activeItemCount;
            _unitValue = totalAmount / _initialPoolSize;
            
            // Compute per-index delay so last item finishes at totalVfxDuration
            float spacingBudget = Mathf.Max(0f, totalVfxDuration - INITIAL_DELAY - bounceDuration - pathDuration);
            _spawnStepDelay = (_activeItemCount > 1) ? spacingBudget / (_activeItemCount - 1) : 0f;
            
            transform.position = currentArgs.startPosition;
            Debug.Log(message:$"start posi: {currentArgs.startPosition}");
            
            EnsurePool(_activeItemCount);
        }

        protected override void ResetValues()
        {
            throw new NotImplementedException();
        }

        private void CreatePool()
        {
            itemList = new List<RectTransform>(_initialPoolSize);
            _pathBuffers.Capacity = _initialPoolSize;
            _itemImages.Capacity = _initialPoolSize;
            EnsurePool(_initialPoolSize);
        }

        private void EnsurePool(int required)
        {
            while (itemList.Count < required)
            {
                var item = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<RectTransform>();
                item.SetParent(itemContainer.transform, true);
                var image = item.GetComponent<Image>();
                image.SetNativeSize();
                item.gameObject.SetActive(false);
                itemList.Add(item);
                _itemImages.Add(image);
            }

            while (_itemImages.Count < itemList.Count)
            {
                _itemImages.Add(itemList[_itemImages.Count].GetComponent<Image>());
            }

            while (_pathBuffers.Count < itemList.Count)
            {
                _pathBuffers.Add(new Vector3[CURVE_POINT_COUNT]);
            }
        }

        #endregion

        private void PlayAnim()
        {
            int itemSizeX = itemSprite.texture.width;

            for (int i = 0; i < _activeItemCount; i++)
            {
                SetupRewardItem(i, itemSizeX);
            }
        }

        private void SetupRewardItem(int index, int itemSizeX)
        {
            int randomSizeX = (int)(Random.Range(SIZE_RANDOM_MIN, SIZE_RANDOM_MAX) * itemSizeX);
            var reward = itemList[index];
            var image = _itemImages[index];

            reward.gameObject.SetActive(true);
            image.SetSizeKeepRatioY(randomSizeX);
            image.sprite = itemSprite;
            image.color = Color.white;
            
            reward.localPosition = new Vector3(Random.Range(-2 * itemSizeX, 2 * itemSizeX), Random.Range(-2 * itemSizeX, 2 * itemSizeX));
            reward.localScale = Vector3.zero;

            float randomScale = Random.Range(SCALE_MIN, SCALE_MAX);

            var sequence = DOTween.Sequence();
            sequence.Append(reward.transform.DOScale(randomScale * 1.2f, 0.3f).SetEase(Ease.InOutSine));
            sequence.Append(reward.transform.DOScale(randomScale, 0.2f).SetEase(Ease.InOutSine));

            if (index == _activeItemCount - 1)
            {
                sequence.OnComplete(OnAllCoinsSpawned);
            }
        }

        private void OnAllCoinsSpawned()
        {
            AutoFindResourceDisplay();

            if (IsHaveRealText)
            {
                realResourceText.gameObject.SetActive(false);
                fakeResourceText.gameObject.SetActive(true);
                fakeResourceText.SetText(NUMBER_FORMAT, prevValue);
            }

            for (int i = 0; i < _activeItemCount; i++)
            {
                AnimateRewardItem(i);
            }
        }

        private void AnimateRewardItem(int index)
        {
            var reward = itemList[index];
            var startPosition = reward.transform.position;
            var distance = targetPosition - startPosition;

            var path = _pathBuffers[index];
            FillCurvePath(index, startPosition, distance, path);

            var randomBouncePosition = reward.localPosition - new Vector3(0, Random.Range(BOUNCE_MIN, BOUNCE_MAX), 0);

            var seq = DOTween.Sequence();
            
            // Delay between items is dynamically spaced to keep total time constant:
            float delay = INITIAL_DELAY + _spawnStepDelay * index;
            
            seq.Append(reward.transform.DOLocalMove(randomBouncePosition, 0.3f).SetDelay(delay).SetEase(Ease.InOutSine));
            seq.Append(reward.transform.DOPath(path, pathDuration, PathType.CatmullRom).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _remainingItems--;
                DebugLogger.Log(message:$"remain Items: {_remainingItems}");
                
                // Animate the last item
                if (_remainingItems <= 0)
                {
                    if (IsHaveRealText)
                    {
                        realResourceText.gameObject.SetActive(true);
                        fakeResourceText.gameObject.SetActive(false);
                        fakeResourceText.transform.SetParent(transform);
                        realResourceText.SetText(NUMBER_FORMAT, prevValue + totalAmount);
                    }
                    
                    try 
                    {
                        currentArgs.OnComplete?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error in OnComplete callback: {ex.Message}\n{ex.StackTrace}");
                    }
                    
                    Recycle();
                }

                DebugLogger.Log(message: $"About trigger some effects");
                
                try 
                {
                    currentArgs.OnItemInteract?.Invoke(); // This will add the methods in events into the call-stack
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnItemInteract callback: {ex.Message}\n{ex.StackTrace}");
                }
                
                DebugLogger.Log(message: $"About trigger some effects 2");
                ApllyPunchEffect();
                
                UpdateFakeResourceText();

                reward.gameObject.SetActive(false);
            }));
            
        }

        private void UpdateFakeResourceText()
        {
            DebugLogger.Log();
            if (IsHaveRealText)
            {
                DebugLogger.Log(message:$"Update fake text: {prevValue + totalAmount - _remainingItems * _unitValue}");
                fakeResourceText.SetText(NUMBER_FORMAT, prevValue + totalAmount - _remainingItems * _unitValue);
            }
        }

        private void ApllyPunchEffect()
        {
            DebugLogger.Log();
            if (targetInteractTransform == null) return;

            float now = Time.time;
            if (s_LastPunchTime.TryGetValue(targetInteractTransform, out float lastTime) && (now - lastTime) < PUNCH_THROTTLE_INTERVAL)
            {
                return;
            }
            s_LastPunchTime[targetInteractTransform] = now;

            if (!s_OriginalScales.TryGetValue(targetInteractTransform, out Vector3 baseScale))
            {
                baseScale = targetInteractTransform.localScale;
                s_OriginalScales[targetInteractTransform] = baseScale;
            }

            targetInteractTransform.DOKill(false);
            targetInteractTransform.localScale = baseScale;

            targetInteractTransform.DOPunchScale(baseScale * PUNCH_SCALE_MULTIPLIER, PUNCH_DURATION, vibrato: 6, elasticity: 0.5f)
                .SetTarget(targetInteractTransform)
                .OnComplete(() =>
                {
                    if (targetInteractTransform != null)
                    {
                        targetInteractTransform.localScale = baseScale;
                        s_OriginalScales.Remove(targetInteractTransform);
                        s_LastPunchTime.Remove(targetInteractTransform);
                    }
                })
                .OnKill(() =>
                {
                    if (targetInteractTransform != null)
                    {
                        targetInteractTransform.localScale = baseScale;
                    }
                });
        }

        private enum CurveType
        {
            EXPONENTIAL = 0,
            SINE = 1,
            PARABOLIC = 2,
            LINEAR = 3,
            LOGARITHMIC = 4,
            // BOUNCE = 5,
            /*ZIGZAG = 6,
            CIRCULAR = 7*/
            COUNT
        }
        
        private void FillCurvePath(int coinIndex, Vector3 startPosition, Vector3 distance, Vector3[] path)
        {
            // Create different curve types based on coin index
            CurveType curveType = (CurveType)(coinIndex % (int)CurveType.COUNT);
    
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float x = (float)j / (CURVE_POINT_COUNT - 1);
                float y = 0f;
        
                switch (curveType)
                {
                    case CurveType.EXPONENTIAL:
                        y = EvaluateSaturationCurve(x, CURVE_STRENGTH);
                        break;
                    case CurveType.SINE:
                        y = Mathf.Sin(x * Mathf.PI * 0.5f) * 1.2f; // Arc shape
                        break;
                    case CurveType.PARABOLIC:
                        y = x * x * 1.5f; // Steeper at end
                        break;
                    case CurveType.LINEAR:
                        y = x; // Straight line
                        break;
                    case CurveType.LOGARITHMIC:
                        y = Mathf.Log10(1 + 9 * x); // log curve, starts slow, ends fast
                        break;
                    /*case CurveType.BOUNCE:
                        y = Mathf.Abs(Mathf.Sin(3 * Mathf.PI * x)) * (1 - x); // bouncy effect
                        break;
                    case CurveType.ZIGZAG:
                        y = (j % 2 == 0) ? 0.2f : 0.8f; // sharp zigzag
                        break;
                    case CurveType.CIRCULAR:
                        y = 1 - Mathf.Sqrt(1 - x * x); // quarter circle
                        break;*/
                }
        
                // Add some randomness to each point
                float randomOffset = Random.Range(-0.1f, 0.1f);
                y = Mathf.Clamp01(y + randomOffset);
        
                path[j] = new Vector3(startPosition.x + x * distance.x, startPosition.y + y * distance.y, startPosition.z);
            }
        }

        // Exponential saturation curve: y = maxY * (1 - e^(-k * x))
        private float EvaluateSaturationCurve(float x, float k, float yMax = 1f)
        {
            return yMax * (1f - Mathf.Exp(-k * x));
        }

        private void AutoFindResourceDisplay()
        {
            if (!IsHaveRealText) return;
    
            fakeResourceText.CopyProperties(realResourceText);
            fakeResourceText.transform.SetParent(realResourceText.transform.parent);
            fakeResourceText.rectTransform.localPosition = realResourceText.rectTransform.localPosition;
            fakeResourceText.rectTransform.sizeDelta = realResourceText.rectTransform.sizeDelta;
            fakeResourceText.transform.localScale = realResourceText.transform.localScale;
        }
    }
}