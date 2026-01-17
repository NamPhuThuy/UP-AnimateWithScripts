using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_LoadingMilestoneBar : AnimationBase
    {
        [Header("Components")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Transform milestoneContainer;
        [SerializeField] private GameObject milestonePrefab;

        [Header("Settings")]
        [SerializeField] private Color reachedColor = Color.green;
        [SerializeField] private Color unreachedColor = Color.gray;

        private List<Image> _milestoneImages = new List<Image>();
        private LoadingMilestoneArgs _currentArgs;
        private Tween _progressTween;

        public override void Play<T>(T args)
        {
            /*if (args is not LoadingMilestoneArgs loadingArgs)
            {
                Debug.LogError("Invalid arguments for Anim_LoadingMilestoneBar");
                return;
            }

            _currentArgs = loadingArgs;
            gameObject.SetActive(true);
            
            SetupMilestones();
            AnimateProgress();*/
        }

        protected override void SetValues()
        {
            // Reset slider to initial state
            if (progressSlider != null)
            {
                progressSlider.value = _currentArgs.initialProgress;
            }
        }

        protected override void ResetValues()
        {
            _progressTween?.Kill();
            foreach (var img in _milestoneImages)
            {
                img.color = unreachedColor;
                img.transform.localScale = Vector3.one;
            }
        }

        private void SetupMilestones()
        {
            // Clear existing milestones
            foreach (Transform child in milestoneContainer)
            {
                Destroy(child.gameObject);
            }
            _milestoneImages.Clear();

            if (_currentArgs.milestones == null) return;

            // Create new milestones
            foreach (float milestoneValue in _currentArgs.milestones)
            {
                GameObject go = Instantiate(milestonePrefab, milestoneContainer);
                
                // Position the milestone based on the slider's width
                RectTransform rt = go.GetComponent<RectTransform>();
                RectTransform containerRt = milestoneContainer.GetComponent<RectTransform>();
                
                // Assuming horizontal layout where 0 is left and 1 is right
                float xPos = (milestoneValue * containerRt.rect.width) - (containerRt.rect.width * 0.5f);
                rt.anchoredPosition = new Vector2(xPos, 0);

                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.color = milestoneValue <= _currentArgs.initialProgress ? reachedColor : unreachedColor;
                    _milestoneImages.Add(img);
                }
            }
        }

        private void AnimateProgress()
        {
            if (progressSlider == null) return;

            _progressTween = DOTween.To(
                () => progressSlider.value,
                x => UpdateProgress(x),
                _currentArgs.targetProgress,
                _currentArgs.duration
            ).SetEase(Ease.Linear)
             .OnComplete(() => 
             {
                 _currentArgs.onComplete?.Invoke();
                 // Optional: Auto-return to pool or stay active
                 // StartAutoReturn(0.5f); 
             });
        }

        private void UpdateProgress(float value)
        {
            progressSlider.value = value;

            // Check milestones
            if (_currentArgs.milestones != null)
            {
                for (int i = 0; i < _currentArgs.milestones.Count; i++)
                {
                    if (value >= _currentArgs.milestones[i])
                    {
                        ActivateMilestone(i);
                    }
                }
            }
        }

        private void ActivateMilestone(int index)
        {
            if (index < 0 || index >= _milestoneImages.Count) return;

            Image img = _milestoneImages[index];
            if (img.color != reachedColor)
            {
                img.color = reachedColor;
                
                // Add a little "pop" animation when reached
                img.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
            }
        }
    }
}
