using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public static class AnimationConst
    {
        public static Vector2 UPPER_ANCHORED = new Vector2(0.5f, 0.88f); 
        public static Vector2 UPPER_MIDDLE_ANCHORED = new Vector2(0.5f, 0.73f); 
        public static Vector2 UPPER_LOW_MIDDLE_ANCHORED = new Vector2(0.5f, 0.58f);
        public static Vector2 BELOW_ANCHORED = new Vector2(0.5f, 0.38f); 
        public static Vector2 CENTER_ANCHORED = new Vector2(0.5f, 0.5f); 
        
        public const float AUTO_RELEASE_ANIM_INTERVAL = 0.5f;

        /// <summary>
        /// Converts a screen percentage (0 to 1 or 0 to 100) to an anchored position relative to a Canvas with bottom-left anchor.
        /// percent.x: 0 = left, 0.5 = center, 1 = right.
        /// percent.y: 0 = bottom, 0.5 = center, 1 = top.
        /// </summary>
        public static Vector2 GetAnchoredPosFromPercent(RectTransform canvasRect, Vector2 percent)
        {
            if (percent.x > 1f || percent.y > 1f) percent /= 100f;

            float x = percent.x * canvasRect.rect.width;
            float y = percent.y * canvasRect.rect.height;

            return new Vector2(x, y);
        }

        public static Vector2 GetAnchoredPosFromPercent(RectTransform canvasRect, float percentX, float percentY)
        {
            return GetAnchoredPosFromPercent(canvasRect, new Vector2(percentX, percentY));
        }

        /// <summary>
        /// Converts a screen percentage (0 to 1 or 0 to 100) to a size relative to Canvas dimensions.
        /// </summary>
        public static Vector2 GetSizeFromPercent(RectTransform canvasRect, Vector2 percent)
        {
            if (percent.x > 1f || percent.y > 1f) percent /= 100f;

            float width = percent.x * canvasRect.rect.width;
            float height = percent.y * canvasRect.rect.height;

            return new Vector2(width, height);
        }

        public static Vector2 GetSizeFromPercent(RectTransform canvasRect, float percentX, float percentY)
        {
            return GetSizeFromPercent(canvasRect, new Vector2(percentX, percentY));
        }
    }
}
