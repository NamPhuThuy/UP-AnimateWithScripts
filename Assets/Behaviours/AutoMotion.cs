using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    public abstract class AutoMotion : MonoBehaviour
    {
        [Header("Flags")]
        [SerializeField] StartIn startIn = StartIn.ON_ENABLE;

        protected enum StartIn
        {
            NOT_AUTO_PLAY = 0,
            ON_ENABLE = 1,
            START = 2,
            
        }
        
        public enum MotionType
        {
            NONE = 0,
            ROTATE = 1,
            CLOCK = 2,
            MOVE = 3,
            SCALE = 4,
        }
        
        public abstract MotionType Type { get; }

        #region MonoBehaviour

        protected virtual void Start()
        {
            if (startIn == StartIn.START)
            {
                Play();
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (startIn == StartIn.START)
            {
                Stop();
            }
        }

        protected virtual void OnEnable()
        {
            if (startIn == StartIn.ON_ENABLE)
            {
                Play();
            }
        }
        
        protected virtual void OnDisable()
        {
            if (startIn == StartIn.ON_ENABLE)
            {
                Stop();
            }
        }

        #endregion

        public abstract void Play();

        public abstract void Stop();

        /// <summary>
        /// Restore the rest pose, then rebuild the motion from the current params.
        /// Stop() kills in-flight tweens first, so spamming Replay never stacks or drifts.
        /// </summary>
        public void Replay()
        {
            Stop();
            Play();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AutoMotion), true)]
    [CanEditMultipleObjects]
    public class AutoMotionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Apply Params & Replay"))
                {
                    foreach (var t in targets)
                        ((AutoMotion)t).Replay();
                }
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to replay the motion.", MessageType.None);
        }
    }
#endif
}