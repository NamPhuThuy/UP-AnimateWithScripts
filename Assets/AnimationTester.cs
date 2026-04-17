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

        [Header("Components")] 
        public Transform dummyUGUI;
        public Transform dummySprite;
        public Camera mainCamera;

        [Header("RESOURCES FLY")] 
        public TextMeshProUGUI coinText;
        public Image coinImage;

        public int itemAmount = 8;
        public int itemSpriteIndex = 0;
        public Sprite[] itemSprites;

        [Header("SPINE CONTROL")] 
        /*public SkeletonAnimation skeletonAnimation;
        public SkeletonGraphic skeletonGraphic;*/
        public string animationName;
        // public Anim_SpineControl.SpineType spineType;

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(AnimationTester))]
    public class AnimationTesterEditor : Editor
    {
        private AnimationTester _script;
        private Texture2D frogIcon;


        private Vector3 testPosition = Vector3.zero;
        private int testAmount = 100;
        private string testMessage = "Test Message";
        private float _testDuration = 2f;
        
        // Positioning
        private bool _useScreenPercentage = false;
        private Vector2 _screenPercentage = new Vector2(50, 50);
        
        // Sprite Motion
        private ObjActiveAuto.MotionType _motionType;

        private bool isUseVFXManagerPos = false;

        #region Callbacks

        private void OnEnable()
        {
            _script = (AnimationTester)target;
            frogIcon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("General Attributes", EditorStyles.boldLabel);

            // Test parameters
            testPosition = EditorGUILayout.Vector3Field("Position", testPosition);
            testAmount = EditorGUILayout.IntField("Amount", testAmount);
            testMessage = EditorGUILayout.TextField("Message", testMessage);
            _testDuration = EditorGUILayout.FloatField("Duration", _testDuration);
            isUseVFXManagerPos = EditorGUILayout.Toggle("Use VFXManager Position", isUseVFXManagerPos);
            
            EditorGUILayout.LabelField("Positioning", EditorStyles.boldLabel);
            _useScreenPercentage = EditorGUILayout.Toggle("Use Screen % (Toast/Popup)", _useScreenPercentage);
            if (_useScreenPercentage)
            {
                _screenPercentage = EditorGUILayout.Vector2Field("Screen % (X,Y)", _screenPercentage);
            }
            
            EditorGUILayout.LabelField("Sprite Motion", EditorStyles.boldLabel);
            _motionType = (ObjActiveAuto.MotionType)EditorGUILayout.EnumPopup("Motion Type", _motionType);

            EditorGUILayout.Space(5);

            ButtonPopupText();
            ButtonPopupImage();
            ButtonPlayItemFly();
            ButtonStatChange();
            ButtonScreenShake();
            ButtonParticleSystem();
            ButtonSpriteMotion();
            ButtonSpineControl();

            // Quick test all VFX types
            EditorGUILayout.Space(5);
        }

        #endregion

        #region VFX-Buttons

        private void ButtonPopupText()
        {
            if (GUILayout.Button(new GUIContent("Play Toast", frogIcon)))
            {
                var args = new ToastArgs
                {
                    message = testMessage,
                    textColor = Color.white,
                    customDuration = 1f,
                    useScreenPercentage = _useScreenPercentage,
                    screenPercentage = _screenPercentage,
                    customAnchoredPos = AnimationConst.UPPER_ANCHORED_POS // Fallback
                };
                AnimationManager.Ins.Play(args);
            }
        }
        
        private void ButtonPopupImage()
        {
            if (GUILayout.Button(new GUIContent("Play Popup Image", frogIcon)))
            {
                int randomIndex = Random.Range(0, _script.itemSprites.Length);
                Vector2 randAnchoredPosi = new Vector2(Random.Range(-500f, 500f), Random.Range(-700f, 700f));
                
                var args = new PopupImageArgs()
                {
                    sprite = _script.itemSprites[randomIndex],
                    useScreenPercentage = _useScreenPercentage,
                    screenPercentage = _screenPercentage,
                    anchoredPos = randAnchoredPosi, // Fallback
                    isUseAnchoredPos = !_useScreenPercentage
                };
                AnimationManager.Ins.Play(args);
            }
        }

        private void ButtonPlayItemFly()
        {
            if (GUILayout.Button(new GUIContent("Play Item Fly", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;

                var args = new ItemFlyArgs
                {
                    addValue = testAmount,
                    prevValue = 0,
                    targetText = coinText.transform,
                    startPosition = AnimationManager.Ins.transform.position,
                    targetInteractTransform = coinPanel.transform, // For positioning the target
                    itemAmount = _script.itemAmount,
                    itemSprite = _script.itemSprites[_script.itemSpriteIndex],
                    OnItemInteract = () => TurnOnStatChangeVFX(coinText),
                    OnComplete = () => DebugLogger.Log(message:"Animation complete!")
                };

                AnimationManager.Ins.Play(args);
            }

            void TurnOnStatChangeVFX(Transform coinText)
            {
                DebugLogger.Log();
                var args = new StatChangeTextArgs
                {
                    amount = testAmount / _script.itemAmount,
                    customColor = Color.yellow,
                    rectTransformOffset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    isUseAnchoredPos = false,
                    targetObject = coinText.gameObject,
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

                var args = new StatChangeTextArgs
                {
                    amount = 5,
                    customColor = Color.yellow,
                    rectTransformOffset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    targetObject = _script.dummyUGUI.gameObject,
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
                    intensity = 0.5f,
                    duration = 0.3f,
                    shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0),
                    OnComplete = null
                });
            }
        }

        private void ButtonSpriteMotion()
        {
            if (GUILayout.Button(new GUIContent("Play Sprite Motion", frogIcon)))
            {
                var args = new SpriteMotionArgs()
                {
                    sprite = _script.itemSprites[6],
                    motionType = _motionType,
                    worldSpaceStartPosi = _script.mainCamera.ScreenToWorldPoint(_script.dummyUGUI.position),
                    customDuration = _testDuration,
                    OnComplete = null
                };

                AnimationManager.Ins.Play(args);
            }
        }
        
        private void ButtonParticleSystem()
        {
            if (GUILayout.Button(new GUIContent("Play Particle System", frogIcon)))
            {
                int randomIndex = Random.Range(0, _script.itemSprites.Length);
                var args = new ParticleSystemArgs()
                {
                    particleSystem = _script.GetComponent<ParticleSystem>(),
                    anchoredPos = Vector2.zero,
                    fromWorld = true,
                    customTexture = _script.itemSprites[randomIndex].texture,
                    OnComplete = null
                };

                AnimationManager.Ins.Play(args);
            }
        }
        
        private void ButtonSpineControl()
        {
            /*if (GUILayout.Button(new GUIContent("Play Spine Control", frogIcon)))
            {
                var args = new SpineControlArgs()
                {
                    skeletonAnimation = _script.skeletonAnimation,
                    activeAnimName = _script.animationName,
                    spineType = _script.spineType,
                    OnComplete = null
                };

                AnimationManager.Ins.Play(args);
            }*/
        }

        #endregion
    }
#endif
}