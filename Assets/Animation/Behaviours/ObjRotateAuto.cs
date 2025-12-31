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
    public class ObjRotateAuto : ObjActiveAuto
    {
        [Header("Flags")]
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private RotateMode rotateMode = RotateMode.LocalAxisAdd;
        
        [Header("Stats")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private Vector3 rotateBy = new Vector3(0f, 0f, 360f);
     

        private Tween _currentTween;
        private Vector3 _rootLocalEulerAngles;

        protected override void Start()
        {
            base.Start();
            _rootLocalEulerAngles = transform.localEulerAngles;
        }

        public override void Play()
        {
            _currentTween?.Kill();

            _currentTween = transform
                .DOLocalRotate(rotateBy, duration, rotateMode)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        public override void Stop()
        {
            _currentTween?.Kill();
            _currentTween = null;

            transform.localEulerAngles = _rootLocalEulerAngles;
        }

        public void ResetValues()
        {
            duration = 1f;
            rotateBy = new Vector3(0f, 0f, 360f);
            ease = Ease.Linear;
            rotateMode = RotateMode.LocalAxisAdd;
        }
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(ObjRotateAuto))]
    [CanEditMultipleObjects]
    public class ObjRotateAutoEditor : Editor
    {
        private ObjRotateAuto script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (ObjRotateAuto)target;

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
