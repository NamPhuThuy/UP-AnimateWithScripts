using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public static class AnimationConst
    {
        public static Vector2 UPPER_ANCHORED_POS = new Vector2(0f, 730f);
        public static Vector2 UPPER_MIDDLE_ANCHORED_POS = new Vector2(0f, 450f);
        public static Vector2 BELOW_ANCHORED_POS = new Vector2(0f, -300f);
        public const float UPPER_ANCHORED_PERCENT = 0.6f; // 60%
        public const float BELOW_ANCHORED_PERCENT = 0.3f; // 30%
        
        public const float AUTO_RELEASE_ANIM_INTERVAL = 0.5f;

        /// <summary>
        /// Converts a screen percentage (0 to 1) to an anchored position relative to a Canvas.
        /// </summary>
        /// <param name="canvasRect">The RectTransform of the root Canvas.</param>
        /// <param name="percentX">Horizontal percentage (0 = left, 0.5 = center, 1 = right).</param>
        /// <param name="percentY">Vertical percentage (0 = bottom, 0.5 = center, 1 = top).</param>
        /// <returns>The calculated anchored position.</returns>
        public static Vector2 GetAnchoredPosFromPercent(RectTransform canvasRect, float percentX, float percentY)
        {
            if (canvasRect == null) return Vector2.zero;

            float x = (percentX - 0.5f) * canvasRect.rect.width;
            float y = (percentY - 0.5f) * canvasRect.rect.height;

            return new Vector2(x, y);
        }

        /*/// <summary>
        /// Helper to get anchored position using the main Canvas from GUIManager.
        /// </summary>
        public static Vector2 GetAnchoredPosFromPercent(float percentX, float percentY)
        {
            var guiManager = NamPhuThuy.UGUIImplement.GUIManager.Ins;
            if (guiManager == null) return Vector2.zero;

            RectTransform canvasRect = guiManager.GetComponent<RectTransform>();
            return GetAnchoredPosFromPercent(canvasRect, percentX, percentY);
        }*/
    }
}
