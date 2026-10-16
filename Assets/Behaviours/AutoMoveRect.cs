using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// RectTransform version of AutoMove.
    /// Waypoints are OFFSETS from the anchoredPosition set in the editor, so the original
    /// anchored layout is never overwritten; Stop() restores it exactly.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutoMoveRect : AutoMotion
    {
        [Header("Flags")]
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Stats")]
        [Min(0.01f)]
        [SerializeField] private float durationPerSegment = 0.5f;

        // Offsets relative to the rest anchoredPosition (UI units).
        [SerializeField] private List<Vector2> waypointOffsets = new List<Vector2>
        {
            new Vector2(-50f, 0f),
            new Vector2( 50f, 0f),
        };

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [Min(0.001f)]
        [SerializeField] private float gizmoRadius = 5f;
        [SerializeField] private Color gizmoPointColor = new Color(0.2f, 0.9f, 1f, 0.95f);
        [SerializeField] private Color gizmoLineColor = new Color(0.2f, 1f, 0.4f, 0.9f);

        private RectTransform _rect;
        private Sequence _currentSeq;
        private Vector2 _restAnchoredPos;

        public override MotionType Type { get => MotionType.MOVE; }

        private void Awake()
        {
            // Cache in Awake: OnEnable (which may call Play) runs before Start
            _rect = (RectTransform)transform;
            _restAnchoredPos = _rect.anchoredPosition;
        }

        public override void Play()
        {
            HandleSpamClick();

            if (waypointOffsets.Count < 2) return;

            _rect.anchoredPosition = _restAnchoredPos + waypointOffsets[0];

            _currentSeq = DOTween.Sequence();
            for (int i = 1; i < waypointOffsets.Count; i++)
            {
                _currentSeq.Append(_rect.DOAnchorPos(_restAnchoredPos + waypointOffsets[i], durationPerSegment).SetEase(ease));
            }

            // Close the loop back to the first waypoint.
            _currentSeq.Append(_rect.DOAnchorPos(_restAnchoredPos + waypointOffsets[0], durationPerSegment).SetEase(ease));

            _currentSeq.SetLoops(-1, LoopType.Restart);
            _currentSeq.SetUpdate(useUnscaledTime);
            _currentSeq.SetLink(gameObject);
        }

        public override void Stop()
        {
            HandleSpamClick();
        }

        /// <summary>
        /// Kills any in-flight sequence and snaps back to the rest anchoredPosition,
        /// so repeated Play/Stop calls never drift the layout.
        /// </summary>
        public void HandleSpamClick()
        {
            _currentSeq?.Kill();
            _currentSeq = null;
            _rect.anchoredPosition = _restAnchoredPos;
        }

        public void ResetValues()
        {
            ease = Ease.InOutSine;
            useUnscaledTime = true;
            durationPerSegment = 0.5f;

            waypointOffsets = new List<Vector2>
            {
                new Vector2(-50f, 0f),
                new Vector2( 50f, 0f),
            };

            drawGizmos = true;
            gizmoRadius = 5f;
            gizmoPointColor = new Color(0.2f, 0.9f, 1f, 0.95f);
            gizmoLineColor = new Color(0.2f, 1f, 0.4f, 0.9f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || waypointOffsets.Count == 0) return;

            RectTransform rect = (RectTransform)transform;
            Transform parent = rect.parent;

            // anchoredPosition deltas equal localPosition deltas, so rebuild the rest localPosition
            Vector2 rest = Application.isPlaying ? _restAnchoredPos : rect.anchoredPosition;
            Vector3 restLocal = rect.localPosition - (Vector3)(rect.anchoredPosition - rest);

            Vector3 OffsetToWorld(Vector2 offset)
            {
                return parent.TransformPoint(restLocal + (Vector3)offset);
            }

            Gizmos.color = gizmoLineColor;
            for (int i = 0; i < waypointOffsets.Count - 1; i++)
            {
                Gizmos.DrawLine(OffsetToWorld(waypointOffsets[i]), OffsetToWorld(waypointOffsets[i + 1]));
            }

            if (waypointOffsets.Count > 1)
            {
                Gizmos.DrawLine(OffsetToWorld(waypointOffsets[^1]), OffsetToWorld(waypointOffsets[0]));
            }

            Gizmos.color = gizmoPointColor;
            float radius = gizmoRadius * parent.lossyScale.x;
            for (int i = 0; i < waypointOffsets.Count; i++)
            {
                Gizmos.DrawSphere(OffsetToWorld(waypointOffsets[i]), radius);
            }
        }
    }
}
