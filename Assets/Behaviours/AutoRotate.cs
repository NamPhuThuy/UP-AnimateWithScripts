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
    public class AutoRotate : AutoMotion
    {
        [Header("Flags")]
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private RotateMode rotateMode = RotateMode.FastBeyond360;
        [SerializeField] private LoopType loopType = LoopType.Incremental;
        
        [Header("Stats")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private Vector3 rotateBy = new Vector3(0f, 0f, 360f);

        private Tween _currentTween;
        private Vector3 _rootLocalEulerAngles;

        public override MotionType Type => MotionType.ROTATE;

        private void Reset()
        {
            ResetValues();
        }

        protected virtual void Awake()
        {
            _rootLocalEulerAngles = transform.localEulerAngles;
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Play()
        {
            CompleteExistingTweens();

            _currentTween = transform
                .DOLocalRotate(rotateBy, duration, rotateMode)
                .SetRelative(true)
                .SetEase(ease)
                .SetLoops(-1, loopType)
                .SetUpdate(true);
        }

        public override void Stop()
        {
            CompleteExistingTweens();
            transform.localEulerAngles = _rootLocalEulerAngles;
        }

        public void CompleteExistingTweens()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }

        public void ResetValues()
        {
            duration = 1f;
            rotateBy = new Vector3(0f, 0f, 360f);
            ease = Ease.Linear;
            rotateMode = RotateMode.FastBeyond360;
            loopType = LoopType.Incremental;
        }
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(AutoRotate))]
    [CanEditMultipleObjects]
    public class AutoRotateEditor : Editor
    {
        private AutoRotate script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (AutoRotate)target;

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
