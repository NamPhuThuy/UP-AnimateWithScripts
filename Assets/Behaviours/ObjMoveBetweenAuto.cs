using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// Havent Tested yet
    /// </summary>
    public class ObjMoveBetweenAuto : ObjActiveAuto
    {
        [Header("Flags")]
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Stats")]
        [Min(0.01f)]
        [SerializeField] private float durationPerSegment = 0.5f;

        // Local-space waypoints (relative to parent).
        [SerializeField] private List<Vector3> waypointsLocal = new List<Vector3>
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3( 0.5f, 0f, 0f),
        };

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [Min(0.001f)]
        [SerializeField] private float gizmoRadius = 0.05f;
        [SerializeField] private Color gizmoPointColor = new Color(0.2f, 0.9f, 1f, 0.95f);
        [SerializeField] private Color gizmoLineColor = new Color(0.2f, 1f, 0.4f, 0.9f);

        private Sequence _currentSeq;
        private Vector3 _rootLocalPos;

        public override MotionType Type { get => MotionType.MOVE; }

        protected override void Start()
        {
            base.Start();
            _rootLocalPos = transform.localPosition;
        }

        public override void Play()
        {
            _currentSeq?.Kill();
            _currentSeq = null;

            if (waypointsLocal == null || waypointsLocal.Count < 2)
            {
                return;
            }

            // Start at the first waypoint (local-space).
            transform.localPosition = waypointsLocal[0];

            _currentSeq = DOTween.Sequence();

            for (int i = 1; i < waypointsLocal.Count; i++)
            {
                _currentSeq.Append(
                    transform.DOLocalMove(waypointsLocal[i], durationPerSegment).SetEase(ease)
                );
            }

            // Close the loop back to the first waypoint.
            _currentSeq.Append(
                transform.DOLocalMove(waypointsLocal[0], durationPerSegment).SetEase(ease)
            );

            _currentSeq.SetLoops(-1, LoopType.Restart);
            _currentSeq.SetUpdate(useUnscaledTime);
        }

        public override void Stop()
        {
            _currentSeq?.Kill();
            _currentSeq = null;

            transform.localPosition = _rootLocalPos;
        }

        public void ResetValues()
        {
            ease = Ease.InOutSine;
            useUnscaledTime = true;
            durationPerSegment = 0.5f;

            waypointsLocal = new List<Vector3>
            {
                new Vector3(-0.5f, 0f, 0f),
                new Vector3( 0.5f, 0f, 0f),
            };

            drawGizmos = true;
            gizmoRadius = 0.05f;
            gizmoPointColor = new Color(0.2f, 0.9f, 1f, 0.95f);
            gizmoLineColor = new Color(0.2f, 1f, 0.4f, 0.9f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || waypointsLocal == null || waypointsLocal.Count == 0)
            {
                return;
            }

            // Local-space gizmos: convert to world using the parent transform (like localPosition does).
            Transform parent = transform.parent;

            Vector3 LocalToWorld(Vector3 local)
            {
                return parent != null ? parent.TransformPoint(local) : local;
            }

            Gizmos.color = gizmoLineColor;
            for (int i = 0; i < waypointsLocal.Count - 1; i++)
            {
                Gizmos.DrawLine(LocalToWorld(waypointsLocal[i]), LocalToWorld(waypointsLocal[i + 1]));
            }

            if (waypointsLocal.Count > 1)
            {
                Gizmos.DrawLine(LocalToWorld(waypointsLocal[^1]), LocalToWorld(waypointsLocal[0]));
            }

            Gizmos.color = gizmoPointColor;
            for (int i = 0; i < waypointsLocal.Count; i++)
            {
                Gizmos.DrawSphere(LocalToWorld(waypointsLocal[i]), gizmoRadius);
            }
        }
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(ObjMoveBetweenAuto))]
    [CanEditMultipleObjects]
    public class ObjMoveBetweenAutoEditor : Editor
    {
        private ObjMoveBetweenAuto script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (ObjMoveBetweenAuto)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}
