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
                    Message = "Hello!",
                    CustomAnchoredPos = managerAnchoredPos,
                    TextColor = Color.white,
                    Duration = 1f,
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
                    AddValue = testAmount,
                    PrevValue = 0,
                    Target = coinText.transform,
                    StartPosition = AnimationManager.Ins.transform.position,
                    TargetInteractTransform = coinPanel.transform, // For positioning the target
                    ItemAmount = _script.itemAmount,
                    ItemSprite = _script.itemSprites[_script.itemSpriteIndex],
                    OnItemInteract = () => TurnOnStatChangeVFX(coinText),
                    OnComplete = () => Debug.Log("Animation complete!")
                };
                
                AnimationManager.Ins.Play(args);
            }

            void TurnOnStatChangeVFX(Transform coinText)
            {
                var args = new StatChangeTextArgs {
                    Amount = testAmount / _script.itemAmount,
                    Color = Color.yellow,
                    Offset = Vector2.zero,
                    MoveDistance = new Vector2(0f, 30f),
                    InitialParent = coinText,
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
                    Amount = 5,
                    Color = Color.yellow,
                    Offset = Vector2.zero,
                    MoveDistance = new Vector2(0f, 30f),
                    InitialParent = coinText,
                    OnComplete = null
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
                    Intensity = 0.5f,
                    Duration = 0.3f,
                    ShakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0),
                    OnComplete = null
                });
            }
        }



        #endregion
    }
#endif
}