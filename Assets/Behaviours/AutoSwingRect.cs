using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// RectTransform version of AutoSwing. A RectTransform rotates around its pivot,
    /// so the swing hinge is the pivot. Enable overridePivot to swing around a different
    /// point (e.g. (0.5, 1) = top-center) without visually moving the element.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutoSwingRect : AutoSwing
    {
        [Header("Pivot")]
        [SerializeField] private bool overridePivot;
        [SerializeField] private Vector2 swingPivot = new Vector2(0.5f, 1f);

        private void Awake()
        {
            // Applied in Awake so the hinge is set before OnEnable -> Play() starts swinging
            if (overridePivot)
            {
                SetPivotKeepPosition((RectTransform)transform, swingPivot);
            }
        }

        /// <summary>
        /// Changes the pivot and compensates anchoredPosition so the element stays where it is on screen.
        /// </summary>
        private static void SetPivotKeepPosition(RectTransform rect, Vector2 pivot)
        {
            Vector2 pivotDelta = pivot - rect.pivot;
            Vector3 localOffset = Vector3.Scale(Vector2.Scale(pivotDelta, rect.rect.size), rect.localScale);
            Vector3 parentOffset = rect.localRotation * localOffset;

            rect.pivot = pivot;
            rect.anchoredPosition += (Vector2)parentOffset;
        }

        private void OnDrawGizmosSelected()
        {
            RectTransform rect = (RectTransform)transform;
            Vector2 pivot = overridePivot && !Application.isPlaying ? swingPivot : rect.pivot;

            // Hinge point in local space: rect.rect is already relative to the current pivot
            Vector2 localHinge = rect.rect.min + Vector2.Scale(rect.rect.size, pivot);
            Vector3 worldHinge = rect.TransformPoint(localHinge);

            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.95f);
            Gizmos.DrawSphere(worldHinge, 5f * rect.lossyScale.x);
        }
    }
}
