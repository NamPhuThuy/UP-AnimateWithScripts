/*
Github: https://github.com/NamPhuThuy
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    
    public class AnimationTester : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [Header("RESOURCES FLY")]
        public TextMeshProUGUI coinText;
        public Image coinImage;

        public int itemAmount = 8;
        public int itemSpriteIndex = 0;
        public Sprite[] itemSprites;

        #endregion
    }

#if UNITY_EDITOR
    
    [CustomEditor(typeof(AnimationTester))]
    public class VFXTesterEditor : Editor
    {
        private AnimationTester _script;
        private Texture2D frogIcon;
        
        
        private AnimationType _selectedAnimationType = AnimationType.NONE;
        private Vector3 testPosition = Vector3.zero;
        private int testAmount = 100;
        private string testMessage = "Test Message";
        private float testDuration = 2f;
        private Vector2 managerAnchoredPos = Vector2.zero;
        
        private bool isUseVFXManagerPos = false;

        #region Callbacks

        private void OnEnable()
        {
            _script = (AnimationTester)target;
            managerAnchoredPos = AnimationManager.Ins.GetComponent<RectTransform>().anchoredPosition;
            frogIcon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            // if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("VFX Testing", EditorStyles.boldLabel);

            // VFX Type selection
            _selectedAnimationType = (AnimationType)EditorGUILayout.EnumPopup("VFX Type", _selectedAnimationType);

            // Test parameters
            testPosition = EditorGUILayout.Vector3Field("Position", testPosition);
            testAmount = EditorGUILayout.IntField("Amount", testAmount);
            testMessage = EditorGUILayout.TextField("Message", testMessage);
            testDuration = EditorGUILayout.FloatField("Duration", testDuration);
            isUseVFXManagerPos = EditorGUILayout.Toggle("Use VFXManager Position", isUseVFXManagerPos);

            EditorGUILayout.Space(5);
            
            ButtonPlayPopupText();
            ButtonPlayItemFly();
            ButtonStatChange();
            ButtonScreenShake();
            
            // Quick test all VFX types
            EditorGUILayout.Space(5);
        }

        #endregion

        #region VFX-Buttons

        private void ButtonPlayPopupText()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Popup Text", frogIcon)))
            {
                var args = new PopupTextArgs {
                    message = "Hello!",
                    customAnchoredPos = managerAnchoredPos,
                    color = Color.white,
                    duration = 1f,
                    onComplete = null
                };
                AnimationManager.Ins.Play<PopupTextArgs>(args);
            }
            
        }

        private void ButtonPlayItemFly()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Item Fly", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;
                
                var args = new ItemFlyArgs {
                    addValue = testAmount,
                    prevValue = 0,
                    target = coinText.transform,
                    startPosition = AnimationManager.Ins.transform.position,
                    targetInteractTransform = coinPanel.transform, // For positioning the target
                    itemAmount = _script.itemAmount,
                    itemSprite = _script.itemSprites[_script.itemSpriteIndex],
                    onItemInteract = () => TurnOnStatChangeVFX(coinText),
                    onComplete = () => Debug.Log("Animation complete!")
                };
                
                AnimationManager.Ins.Play(args);
            }

            void TurnOnStatChangeVFX(Transform coinText)
            {
                var args = new StatChangeTextArgs {
                    amount = testAmount / _script.itemAmount,
                    color = Color.yellow,
                    offset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    initialParent = coinText,
                    onComplete = null
                };
                
                AnimationManager.Ins.Play(args);
            }
        }
        
        private void ButtonStatChange()
        {
            if (GUILayout.Button(new GUIContent("Play State Change Text", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;
                
                var args = new StatChangeTextArgs {
                    amount = 5,
                    color = Color.yellow,
                    offset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    initialParent = coinText,
                    onComplete = null
                };
                
                AnimationManager.Ins.Play(args);
            }
        }

        private void ButtonScreenShake()
        {
            if (GUILayout.Button(new GUIContent("Play Screen Shake", frogIcon)))
            {
                AnimationManager.Ins.Play(new ScreenShakeArgs
                {
                    intensity = 0.5f,
                    duration = 0.3f,
                    shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0)
                });
            }
        }



        #endregion
    }
#endif
}