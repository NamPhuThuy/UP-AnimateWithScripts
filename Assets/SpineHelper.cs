using System.Collections;
using Spine.Unity;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public static class SpineHelper
    {
        public static void ChangeSkin(Spine.Unity.SkeletonGraphic skeletonGraphic, string skinName)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return;
        
            var skeleton = skeletonGraphic.Skeleton;
        
            // Try to find the skin by name
            var newSkin = skeleton.Data.FindSkin(skinName);
            if (newSkin == null)
            {
                Debug.LogWarning($"Chest skin not found: {skinName}");
                return;
            }
        
            // Apply new skin
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
        
            // Optional: re\-apply current animation to refresh visuals
            var animState = skeletonGraphic.AnimationState;
            var current = animState.GetCurrent(0);
            if (current != null)
            {
                animState.SetAnimation(0, current.Animation.Name, current.Loop);
            }
        
            skeletonGraphic.Update(0);
            skeletonGraphic.LateUpdate();
        }
        
        #region PLAY ANIMATION

        /// <summary>
        /// Plays an animation for UI Spine (SkeletonGraphic)
        /// </summary>
        public static void PlayAnimation(SkeletonGraphic skeletonGraphic, string animationName, bool loop = true, float timeScale = 1f)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return;

            // Optional: Check if animation exists to prevent errors
            if (skeletonGraphic.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonGraphic.name}");
                return;
            }

            skeletonGraphic.timeScale = timeScale;
            skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
        }

        /// <summary>
        /// Plays an animation for World Spine (SkeletonAnimation)
        /// </summary>
        public static void PlayAnimation(SkeletonAnimation skeletonAnimation, string animationName, bool loop = true, float timeScale = 1f)
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
                return;

            // Optional: Check if animation exists to prevent errors
            if (skeletonAnimation.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonAnimation.name}");
                return;
            }

            skeletonAnimation.timeScale = timeScale;
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        }

        #endregion
    }

}