using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Utils
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UITransformTester : MonoBehaviour
    {
        public enum PositionType
        {
            TRANSFORM_POSITION = 0,
            RECT_TRANSFORM_POSITION = 1,
            ANCHORED_POSITION = 2
        }

        public PositionType positionType = PositionType.ANCHORED_POSITION;
        public Vector3 targetPosition;
        public bool liveUpdate = false;

        [Header("Stats")] 
        public Vector3 transformPosition;
        public Vector3 transformLocalPosi;
        public Vector3 rectTransformPosition;
        public Vector3 rectTransformAnchoredPosition;
        public Vector3 rectTransformAnchoredPosition3D;
        public Vector3 rectTransformLocalPosi;

        [Header("Components")] 
        public RectTransform rectTransform;

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            
            transformPosition = transform.position;
            transformLocalPosi = transform.localPosition;
            rectTransformPosition = rectTransform.position;
            rectTransformAnchoredPosition = rectTransform.anchoredPosition;
                
            rectTransformAnchoredPosition3D = rectTransform.anchoredPosition3D;
            rectTransformLocalPosi = rectTransform.localPosition;
            
            if (liveUpdate)
            {
                ApplyPosition();

                
            }
        }

        [ContextMenu("Sync Target From Current")]
        public void SyncTargetFromCurrent()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            switch (positionType)
            {
                case PositionType.TRANSFORM_POSITION:
                    targetPosition = transform.position;
                    break;
                case PositionType.RECT_TRANSFORM_POSITION:
                    targetPosition = rectTransform.position;
                    break;
                case PositionType.ANCHORED_POSITION:
                    targetPosition = rectTransform.anchoredPosition;
                    break;
            }
        }

        public void ApplyPosition()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "UI Transform Test");
            UnityEditor.Undo.RecordObject(rectTransform, "UI Transform Test");
#endif

            switch (positionType)
            {
                case PositionType.TRANSFORM_POSITION:
                    transform.position = targetPosition;
                    break;
                case PositionType.RECT_TRANSFORM_POSITION:
                    rectTransform.position = targetPosition;
                    break;
                case PositionType.ANCHORED_POSITION:
                    rectTransform.anchoredPosition = targetPosition;
                    break;
            }
        }

        public void ApplyAsTransformPosition()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "Apply Transform Position");
#endif
            transform.position = targetPosition;
        }

        public void ApplyAsRectPosition()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(rectTransform, "Apply Rect Position");
#endif
            rectTransform.position = targetPosition;
        }

        public void ApplyAsAnchoredPosition()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(rectTransform, "Apply Anchored Position");
#endif
            rectTransform.anchoredPosition = targetPosition;
        }

        private void OnValidate()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UITransformTester))]
    public class UITransformTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UITransformTester tester = (UITransformTester)target;

            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manual Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Sync Target From Current"))
            {
                tester.SyncTargetFromCurrent();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to Transform.Pos"))
            {
                tester.ApplyAsTransformPosition();
            }
            if (GUILayout.Button("Apply to Rect.Pos"))
            {
                tester.ApplyAsRectPosition();
            }
            if (GUILayout.Button("Apply to Anchored.Pos"))
            {
                tester.ApplyAsAnchoredPosition();
            }
            EditorGUILayout.EndHorizontal();

            if (tester.liveUpdate)
            {
                EditorGUILayout.HelpBox("Live Update is ON. The UI element will constantly snap to Target Position based on the selected Position Type.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("TransformPosition: World space position.\n" +
                                    "RectTransformPosition: Same as transform.position for UI.\n" +
                                    "AnchoredPosition: Position relative to anchors.", MessageType.Info);
        }
    }
#endif
}