using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
   

    [Serializable]
    public class RewardSegment
    {
        public GameObject segmentObject;
        public Image segmentImage;
        public Image fillImage;
        public bool isCompleted;
        public int index;
    }

    public class Anim_RewardProgress : AnimationBase
    {
        #region Private Serializable Fields
        
        [Header("Stats")]
        [SerializeField] private RewardProgressArgs currentArgs;
        [SerializeField] private int totalSegmentNum;
        [SerializeField] private int currentSegmentNum;
        [SerializeField] private int targetSegmentNum;
        
        [Header("Prefabs & Templates")]
        [SerializeField] private GameObject segmentPrefab;
        
        [Header("Containers")]
        [SerializeField] private Transform segmentContainer;
        [SerializeField] private Image prizeImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image startEndpointImage;
        [SerializeField] private Image endEndpointImage;
        
        [Header("Layout Settings")]
        [SerializeField] private float segmentSpacing = 10f;
        [SerializeField] private Vector2 segmentSize = new Vector2(80f, 20f);
        [SerializeField] private bool autoLayout = true;
        
        [Header("Sprites")]
        [SerializeField] private Sprite customEndPointSprite;
        [SerializeField] private Sprite customSegmentSprite;
        [SerializeField] private Sprite customBackgroundSprite;
        [SerializeField] private Sprite prizeSprite;
        
        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI progressText; // "3/10"
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem segmentCompleteParticle;
        [SerializeField] private ParticleSystem rewardClaimParticle;
        
        #endregion
        
        #region Private Fields
        
        private List<RewardSegment> segments = new List<RewardSegment>();
        private Coroutine progressAnimation;
        private Tween prizePulseTween;
        
        #endregion
        
        #region MonoBehaviour Callbacks
        
        private void Awake()
        {
            if (segmentContainer == null)
            {
                segmentContainer = transform.Find("SegmentContainer");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Manually advance one segment
        /// </summary>
        public void AdvanceOneSegment()
        {
            if (currentSegmentNum < totalSegmentNum)
            {
                FillSegment(currentSegmentNum, true);
                currentSegmentNum++;
                UpdateProgressText();
            }
        }
        
        /// <summary>
        /// Set progress to specific segment instantly
        /// </summary>
        public void SetProgressInstant(int segmentIndex)
        {
            for (int i = 0; i < segmentIndex && i < segments.Count; i++)
            {
                FillSegment(i, false);
            }
            currentSegmentNum = segmentIndex;
            UpdateProgressText();
        }
        
        /// <summary>
        /// Check if reward can be claimed
        /// </summary>
        public bool CanClaimReward()
        {
            return currentSegmentNum >= totalSegmentNum;
        }
        
        #endregion
        
        #region Private Methods
        
        private void PlayAnim()
        {
            if (progressAnimation != null)
            {
                StopCoroutine(progressAnimation);
            }
            
            progressAnimation = StartCoroutine(ProgressAnimationRoutine());
        }
        
        private IEnumerator ProgressAnimationRoutine()
        {
            // Initial delay
            yield return new WaitForSeconds(0.3f);
            
            // Show message animation
            if (messageText != null && !string.IsNullOrEmpty(currentArgs.customMessage))
            {
                messageText.transform.localScale = Vector3.zero;
                messageText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.5f);
            }
            
            // Animate from current to target
            int startSegment = currentArgs.currentSegmentNum;
            int endSegment = Mathf.Min(currentArgs.targetSegmentNum, totalSegmentNum);
            
            float fillDuration = currentArgs.segmentFillDuration > 0 
                ? currentArgs.segmentFillDuration 
                : 0.4f;
            
            float delayBetween = currentArgs.delayBetweenSegments > 0 
                ? currentArgs.delayBetweenSegments 
                : 0.15f;
            
            for (int i = startSegment; i < endSegment; i++)
            {
                FillSegment(i, true);
                currentSegmentNum = i + 1;
                UpdateProgressText();
                
                // Callback
                currentArgs.OnSegmentComplete?.Invoke(i);
                
                // Play effects
                if (currentArgs.playSound)
                {
                    PlaySegmentSound();
                }
                
                if (currentArgs.showParticles && segmentCompleteParticle != null)
                {
                    PlaySegmentParticle(i);
                }
                
                yield return new WaitForSeconds(delayBetween);
            }
            
            // Check if reward is unlocked
            if (currentSegmentNum >= totalSegmentNum)
            {
                yield return new WaitForSeconds(0.3f);
                AnimateRewardUnlock();
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Complete
            currentArgs.OnComplete?.Invoke();
        }
        
        private void CreateSegments()
        {
            // Clear existing
            ClearSegments();
            
            if (segmentPrefab == null)
            {
                Debug.LogError("[RewardProgress] Segment prefab is null!");
                return;
            }
            
            for (int i = 0; i < totalSegmentNum; i++)
            {
                CreateSegment(i);
            }
            
            if (autoLayout)
            {
                LayoutSegments();
            }
        }
        
        private void CreateSegment(int index)
        {
            GameObject segmentGO = Instantiate(segmentPrefab, segmentContainer);
            segmentGO.name = $"Segment_{index}";
            
            // Get or create images
            Image segmentImg = segmentGO.GetComponent<Image>();
            if (segmentImg == null)
            {
                segmentImg = segmentGO.AddComponent<Image>();
            }
            
            // Find or create fill image
            Image fillImg = segmentGO.transform.Find("Fill")?.GetComponent<Image>();
            if (fillImg == null)
            {
                GameObject fillGO = new GameObject("Fill");
                fillGO.transform.SetParent(segmentGO.transform, false);
                fillImg = fillGO.AddComponent<Image>();
                
                RectTransform fillRT = fillGO.GetComponent<RectTransform>();
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = Vector2.one;
                fillRT.sizeDelta = Vector2.zero;
                fillRT.anchoredPosition = Vector2.zero;
                
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
            }
            
            fillImg.fillAmount = 0f;
            
            // Apply custom sprite
            if (customSegmentSprite != null)
            {
                segmentImg.sprite = customSegmentSprite;
            }
            
            // Create segment data
            RewardSegment segment = new RewardSegment
            {
                segmentObject = segmentGO,
                segmentImage = segmentImg,
                fillImage = fillImg,
                isCompleted = false,
                index = index
            };
            
            segments.Add(segment);
            
            // Set size
            RectTransform rt = segmentGO.GetComponent<RectTransform>();
            rt.sizeDelta = segmentSize;
        }
        
        private void LayoutSegments()
        {
            float totalWidth = (segmentSize.x * segments.Count) + (segmentSpacing * (segments.Count - 1));
            float startX = -totalWidth / 2f + segmentSize.x / 2f;
            
            for (int i = 0; i < segments.Count; i++)
            {
                RectTransform rt = segments[i].segmentObject.GetComponent<RectTransform>();
                float xPos = startX + i * (segmentSize.x + segmentSpacing);
                rt.anchoredPosition = new Vector2(xPos, 0f);
            }
        }
        
        private void ClearSegments()
        {
            foreach (var segment in segments)
            {
                if (segment.segmentObject != null)
                {
                    Destroy(segment.segmentObject);
                }
            }
            
            segments.Clear();
        }
        
        private void FillSegment(int index, bool animate)
        {
            if (index < 0 || index >= segments.Count) return;
            
            var segment = segments[index];
            
            if (animate)
            {
                // Animated fill
                float duration = currentArgs.segmentFillDuration > 0 
                    ? currentArgs.segmentFillDuration 
                    : 0.4f;
                
                segment.fillImage.DOKill();
                segment.fillImage.DOFillAmount(1f, duration)
                    .SetEase(Ease.OutQuad);
                
                // Scale pulse
                segment.segmentObject.transform.DOKill();
                segment.segmentObject.transform
                    .DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }
            else
            {
                // Instant fill
                segment.fillImage.fillAmount = 1f;
            }
            
            segment.isCompleted = true;
        }
        
        private void AnimateRewardUnlock()
        {
            if (prizeImage == null) return;
            
            // Prize pulse animation
            prizePulseTween?.Kill();
            prizeImage.transform.localScale = Vector3.one;
            
            prizePulseTween = prizeImage.transform
                .DOScale(1.2f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // Glow effect (if you have outline)
            prizeImage.DOColor(Color.yellow, 0.3f)
                .SetLoops(4, LoopType.Yoyo);
            
            // Particle effect
            if (rewardClaimParticle != null)
            {
                rewardClaimParticle.transform.position = prizeImage.transform.position;
                rewardClaimParticle.Play();
            }
        }
        
        private void UpdateProgressText()
        {
            if (progressText != null)
            {
                progressText.text = $"{currentSegmentNum}/{totalSegmentNum}";
            }
        }
        
        private void PlaySegmentSound()
        {
            // TODO: Play audio clip
            // AudioManager.Instance.PlaySFX("segment_complete");
        }
        
        private void PlaySegmentParticle(int segmentIndex)
        {
            if (segmentCompleteParticle == null || segmentIndex >= segments.Count) return;
            
            Vector3 particlePos = segments[segmentIndex].segmentObject.transform.position;
            segmentCompleteParticle.transform.position = particlePos;
            segmentCompleteParticle.Play();
        }
        
        private void SetupEndpoints()
        {
            if (startEndpointImage != null && customEndPointSprite != null)
            {
                startEndpointImage.sprite = customEndPointSprite;
            }
            
            if (endEndpointImage != null)
            {
                if (customEndPointSprite != null)
                {
                    endEndpointImage.sprite = customEndPointSprite;
                }
                
                // Place prize on end
                if (prizeImage != null && prizeSprite != null)
                {
                    prizeImage.sprite = prizeSprite;
                    prizeImage.transform.SetParent(endEndpointImage.transform, false);
                }
            }
        }
        
        #endregion
        
        #region Override Methods
        
        public override void Play<T>(T args)
        {
            if (args is RewardProgressArgs rewardProgressArgs)
            {
                currentArgs = rewardProgressArgs;
                gameObject.SetActive(true);
                SetValues();
                KillTweens();
                CreateSegments();
                SetupEndpoints();
                PlayAnim();
            }
        }
        
        protected override void SetValues()
        {
            // Must-have Values
            totalSegmentNum = currentArgs.totalSegmentNum;
            currentSegmentNum = currentArgs.currentSegmentNum;
            targetSegmentNum = currentArgs.targetSegmentNum;
            
            prizeSprite = currentArgs.prizeSprite;
            
            // Custom Values
            if (currentArgs.customMessage != null && messageText != null)
            {
                messageText.text = currentArgs.customMessage;
            }
            
            if (currentArgs.customSegmentSprite != null)
            {
                customSegmentSprite = currentArgs.customSegmentSprite;
            }
            
            if (currentArgs.customBackgroundSprite != null)
            {
                customBackgroundSprite = currentArgs.customBackgroundSprite;
                
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = customBackgroundSprite;
                }
            }
            
            if (currentArgs.customEndpointSprite != null)
            {
                customEndPointSprite = currentArgs.customEndpointSprite;
            }
            
            // Update progress text
            UpdateProgressText();
        }
        
        protected override void ResetValues()
        {
            currentSegmentNum = 0;
            targetSegmentNum = 0;
            
            ClearSegments();
            
            if (messageText != null)
            {
                messageText.text = "";
            }
            
            if (progressText != null)
            {
                progressText.text = "0/0";
            }
            
            prizePulseTween?.Kill();
            
            if (prizeImage != null)
            {
                prizeImage.transform.localScale = Vector3.one;
                prizeImage.color = Color.white;
            }
        }
        
        protected new void KillTweens()
        {
            base.KillTweens();
            
            if (progressAnimation != null)
            {
                StopCoroutine(progressAnimation);
                progressAnimation = null;
            }
            
            prizePulseTween?.Kill();
            
            foreach (var segment in segments)
            {
                if (segment.segmentObject != null)
                {
                    segment.segmentObject.transform.DOKill();
                }
                
                if (segment.fillImage != null)
                {
                    segment.fillImage.DOKill();
                }
            }
        }
        
        #endregion
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Anim_RewardProgress))]
    [CanEditMultipleObjects]
    public class Anim_RewardProgressEditor : Editor
    {
        private Anim_RewardProgress script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog");
            script = (Anim_RewardProgress)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Advance One Segment"))
            {
                script.AdvanceOneSegment();
            }
            
            if (GUILayout.Button("Set to 50%"))
            {
                int halfSegments = script.GetType()
                    .GetField("totalSegmentNum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(script) as int? ?? 5;
                script.SetProgressInstant(halfSegments / 2);
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Check Can Claim"))
            {
                bool canClaim = script.CanClaimReward();
                EditorUtility.DisplayDialog("Reward Status", 
                    canClaim ? "✅ Reward can be claimed!" : "❌ Not enough progress", 
                    "OK");
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
        }
    }
    #endif
}