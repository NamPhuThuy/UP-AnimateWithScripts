/*
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    public class Anim_SpineControl : AnimationBase
    {
        #region Private Serializable Fields

        public enum SpineType
        {
            NONE = 0,
            UI = 1,
            SPRITE = 2
        }

        [Header("Flags")] 
        [SerializeField] private SpineType spineType;
        
        [Header("Stats")]
        [SerializeField] private Vector3 worldPosi;
        [SerializeField] private Vector3 screenPosi;
        [SerializeField] private string activeAnimName;
        
        [Header("Components")] 
        [SerializeField] private SpineControlArgs currentArgs;

        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        private SkeletonData _skeletonData; 
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void ActiveSpine()
        {
            switch (spineType)
            {
                case SpineType.UI:
                    skeletonGraphic = currentArgs.skeletonGraphic;
                    skeletonAnimation.transform.position = worldPosi;
                    SpineHelper.PlayAnimation(skeletonGraphic, activeAnimName, false);
                    break;
                case SpineType.SPRITE:
                    skeletonAnimation = Instantiate(currentArgs.skeletonAnimation);
                    skeletonAnimation.transform.position = worldPosi;
                    SpineHelper.PlayAnimation(skeletonAnimation, activeAnimName, false);
                    break;
            }
        }
        
        #endregion
        
        #region Override Methods 
        
        public override void Play<T>(T args)
        {
            if (args is  SpineControlArgs spineControlArgs)
            {
                currentArgs = spineControlArgs;
                SetValues();
                ActiveSpine();
                return;
            }
            
           
            
        }

        protected override void SetValues()
        {
            _skeletonData = currentArgs.skeletonData;
            activeAnimName = currentArgs.activeAnimName;
            worldPosi = currentArgs.worldPosi;
        }

        protected override void ResetValues()
        {
            
        }
        
        #endregion

        #region Editor Methods

      

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(Anim_SpineControl))]
    [CanEditMultipleObjects]
    public class Anim_SpineControlEditor : Editor
    {
        private Anim_SpineControl script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
            script = (Anim_SpineControl)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
           

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
    #endif#1#
}
*/
