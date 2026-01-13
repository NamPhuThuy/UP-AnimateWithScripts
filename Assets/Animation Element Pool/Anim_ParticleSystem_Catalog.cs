using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    [CreateAssetMenu(fileName = "Anim_ParticleSystem_Catalog", menuName = "AnimateWithScripts/Anim_ParticleSystem_Catalog", order = 1)]
    public class Anim_ParticleSystem_Catalog : ScriptableObject
    {
        #region Private Serializable Fields

        public enum PSOrder
        {
            FIRST = 0, 
            SECOND = 1, 
            THIRD = 2,
            FOURTH = 3,
            FIFTH = 4,
            SIXTH = 5,
            SEVENTH = 6,
            EIGHTH = 7,
            NINTH = 8,
            TENTH = 9,
            
        }

        [SerializeField] private ParticleSystem[] listPS;
        private Dictionary<PSOrder, ParticleSystem> _dictData;

        public Dictionary<PSOrder, ParticleSystem> DictData
        {
            get
            {
                EnsureInitDict();

                return _dictData;
            }
        }

        #endregion

        #region Private Fields
        
        private void EnsureInitDict()
        {
            if (_dictData != null) return;
            _dictData = new Dictionary<PSOrder, ParticleSystem>(listPS?.Length ?? 0);
            if (_dictData == null) return;

            if (listPS == null)
            {
                return;
            }
            
            for (var i = 0; i < listPS.Length; i++)
            {
                if (listPS[i] == null)
                {
                    Debug.Log(message: $"PSList.EnsureDictInit: null PS found, skipping");
                    continue;
                }

                _dictData[(PSOrder)i] = listPS[i];
            }
        }

        #endregion

        #region MonoBehaviour Callbacks

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(Anim_ParticleSystem_Catalog))]
    [CanEditMultipleObjects]
    public class Anim_ParticleSystem_CatalogEditor : Editor
    {
        private Anim_ParticleSystem_Catalog script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
            script = (Anim_ParticleSystem_Catalog)target;
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
    #endif*/
}
