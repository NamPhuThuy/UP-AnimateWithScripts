#if TEST_FRAMEWORK_INSTALLED && PRIME_TWEEN_PRO
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using PrimeTween;
using TMPro;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Assert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Tests {
    [Test]
    public void TweenAnimationChildEase() {
        transform.position = Vector3.one * 10;
        Vector3 startValue = Random.onUnitSphere;
        Vector3 endValue = Random.onUnitSphere;
        var stepAnimationCurve = new AnimationCurve(
            new Keyframe(0f, 0f) { inTangent = float.PositiveInfinity, outTangent = float.PositiveInfinity },
                       new Keyframe(1f, 1f) { inTangent = float.PositiveInfinity, outTangent = float.PositiveInfinity });
        var tweenAnimation = CreateTweenAnimation(startValue, endValue, stepAnimationCurve);
        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.YoyoChildren;
        CheckForwardCycle();
        void CheckForwardCycle() {
            Assert.IsFalse(tweenAnimation.isForward);
            tweenAnimation.Play();
            Assert.IsTrue(tweenAnimation.isForward);
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(startValue == transform.position);
            seq.progressTotal = 0.5f;
            Assert.IsTrue(startValue == transform.position);
            seq.progressTotal = 1f;
            Assert.IsTrue(endValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }
        {
            tweenAnimation.Play();
            Assert.IsFalse(tweenAnimation.isForward);
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(endValue == transform.position);
            seq.progressTotal = 0.5f;
            Assert.IsTrue(endValue == transform.position);
            seq.progressTotal = 1f;
            Assert.IsTrue(startValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }

        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Yoyo;
        CheckForwardCycle();
        CheckBackwardCycle();
        void CheckBackwardCycle() {
            tweenAnimation.Play();
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(endValue == transform.position);
            seq.progressTotal = 0.5f;
            Assert.IsTrue(startValue == transform.position);
            seq.progressTotal = 1f;
            Assert.IsTrue(startValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }

        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Rewind;
        CheckForwardCycle();
        CheckBackwardCycle();
    }

    TweenAnimation CreateTweenAnimation(Vector3 startValue, Vector3 endValue, Easing childEasing = default, float duration = 1f) {
        return new TweenAnimation {
            animations = new List<TweenAnimation.Data> {
                new TweenAnimation.Data {
                    target = transform,
                    startValue = startValue.ToContainer(),
                    endValue = endValue.ToContainer(),
                    tweenType = TweenAnimation.TweenType.Position,
                    duration = duration,
                    ease = childEasing.ease,
                    customEase = childEasing.curve
                }
            }
        };
    }

    [Test]
    public void TweenAnimationEase() {
        Vector3 startValue = Random.onUnitSphere;
        Vector3 endValue = Random.onUnitSphere;
        var tweenAnimation = CreateTweenAnimation(startValue, endValue, Ease.Linear);
        tweenAnimation.sequenceEase = Ease.OutExpo;
        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Yoyo;

        CheckForwardCycle();
        void CheckForwardCycle() {
            tweenAnimation.Play();
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(startValue == transform.position);
            seq.progressTotal = 0.5f;
            var t = seq.root.tween.next;
            Assert.AreEqual(t.managedData.target, transform);
            Assert.IsTrue(new Tween(t).progress > 0.9f);
            seq.progressTotal = 1f;
            Assert.IsTrue(endValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }
        CheckBackwardCycle();
        void CheckBackwardCycle() {
            tweenAnimation.Play();
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(endValue == transform.position);
            seq.progressTotal = 0.5f;
            var t = seq.root.tween.next;
            Assert.AreEqual(t.managedData.target, transform);
            Assert.IsTrue(new Tween(t).progress < 0.1f);
            seq.progressTotal = 1f;
            Assert.IsTrue(startValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }

        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.YoyoChildren;
        CheckForwardCycle();
        CheckBackwardCycle();

        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Rewind;
        CheckForwardCycle();
        {
            tweenAnimation.Play();
            var seq = tweenAnimation._sequence;
            Assert.IsTrue(seq.isAlive);
            seq.progressTotal = 0f;
            Assert.IsTrue(endValue == transform.position);
            seq.progressTotal = 0.5f;
            var t = seq.root.tween.next;
            Assert.AreEqual(t.managedData.target, transform);
            Assert.IsTrue(new Tween(t).progress > 0.9f);
            seq.progressTotal = 1f;
            Assert.IsTrue(startValue == transform.position);
            Assert.IsFalse(seq.isAlive);
        }

        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Restart;
        CheckForwardCycle();
        CheckForwardCycle();
        CheckForwardCycle();
    }

    [UnityTest]
    public IEnumerator TweenAnimationCallback() {
        var tweenAnimation = CreateTweenAnimation(Vector3.zero, Vector3.one, Ease.Linear);
        var callback = new TweenAnimation.Data.Custom.Callback();
        int numCallback = 0;
        callback.unityEvent.AddListener(() => {
            numCallback++;
            // print("done");
        });
        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Restart;
        tweenAnimation.animations = new List<TweenAnimation.Data> {
            new TweenAnimation.Data {
                operation = TweenAnimation.Operation.Chain,
                tweenType = TweenAnimation.TweenType.Callback,
                duration = 0f,
                customData = callback
            },
            tweenAnimation.animations[0]
        };

        tweenAnimation.Play();
        var seq = tweenAnimation._sequence;
        Assert.IsTrue(seq.isAlive);

        // EditorApplication.isPaused = true;
        // yield return null;

        Assert.AreEqual(1f, seq.duration);
        seq.progress = 1f;
        // yield return seq.ToYieldInstruction();
        Assert.IsFalse(seq.isAlive);
        Assert.AreEqual(1, numCallback);

        // restart
        tweenAnimation.Play();
        tweenAnimation._sequence.progress = 1f;
        // yield return tweenAnimation.ToYieldInstruction();
        Assert.AreEqual(2, numCallback);

        // yoyo backward from the completed state
        tweenAnimation.cycleMode = Sequence.SequenceCycleMode.Yoyo;
        tweenAnimation.Play();
        tweenAnimation._sequence.progress = 1f;
        // yield return tweenAnimation.ToYieldInstruction();
        Assert.AreEqual(2, numCallback); // callback is not called on the backward cycle

        // yoyo forward
        // print("yoyo backward");
        tweenAnimation.Play();
        yield return null;
        tweenAnimation._sequence.progress = 1f;
        // yield return tweenAnimation.ToYieldInstruction();
        Assert.AreEqual(3, numCallback);

        yield return null;
    }

    [Test]
    public void CycleModeEnum() {
        Assert.AreEqual((int)_CycleMode.Restart, (int)Sequence.SequenceCycleMode.Restart);
        Assert.AreEqual((int)_CycleMode.Yoyo, (int)Sequence.SequenceCycleMode.Yoyo);
        Assert.AreEqual((int)_CycleMode.Rewind, (int)Sequence.SequenceCycleMode.Rewind);
        Assert.AreEqual((int)_CycleMode.YoyoChildren, (int)Sequence.SequenceCycleMode.YoyoChildren);
        Assert.AreEqual((CycleMode)_CycleMode.YoyoChildren, (CycleMode)_CycleMode.YoyoChildren);

        Assert.AreEqual((int)_CycleMode.Restart, (int)CycleMode.Restart);
        Assert.AreEqual((int)_CycleMode.Yoyo, (int)CycleMode.Yoyo);
        Assert.AreEqual((int)_CycleMode.Incremental, (int)CycleMode.Incremental);
        Assert.AreEqual((int)_CycleMode.Rewind, (int)CycleMode.Rewind);
    }

    [Test]
    public void SequenceCycleModeIncremental() {
        LogAssert.Expect(LogType.Error, new Regex("Sequence doesn't support CycleMode.Incremental"));
        Sequence.Create(1, (Sequence.SequenceCycleMode)CycleMode.Incremental);
    }

    [UnityTest]
    public IEnumerator TweenAnimationComponentAnimations() {
        Type inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        new GameObject(nameof(AudioListener)).AddComponent<AudioListener>();
        bool testInspector = Random.value > 0.0f;
        var tweenTypes = Enum.GetValues(typeof(TweenAnimation.TweenType)).Cast<TweenAnimation.TweenType>();
        // TweenAnimation.TweenType[] tweenTypes = { TweenAnimation.TweenType.UIHorizontalNormalizedPosition };
        foreach (var tweenType in tweenTypes) {
            // Debug.Log(tweenType);
            (PropType propType, Type targetType) = Utils.TweenTypeToTweenData(tweenType);
            if (propType == PropType.Quaternion) {
                continue;
            }
            switch (tweenType) {
                case TweenAnimation.TweenType.Disabled:
                case TweenAnimation.TweenType.MainSequence:
                case TweenAnimation.TweenType.NestedSequence:
                case TweenAnimation.TweenType.TweenTimeScale:
                case TweenAnimation.TweenType.TweenTimeScaleSequence:
                case TweenAnimation.TweenType.VisualElementLayout:
                case TweenAnimation.TweenType.VisualElementPosition:
                case TweenAnimation.TweenType.VisualElementRotationQuaternion:
                case TweenAnimation.TweenType.VisualElementScale:
                case TweenAnimation.TweenType.VisualElementSize:
                case TweenAnimation.TweenType.VisualElementTopLeft:
                case TweenAnimation.TweenType.VisualElementColor:
                case TweenAnimation.TweenType.VisualElementBackgroundColor:
                case TweenAnimation.TweenType.VisualElementOpacity:
                #if PRIME_TWEEN_EXPERIMENTAL
                case TweenAnimation.TweenType.CustomDouble:
                #endif
                case TweenAnimation.TweenType.TweenAwaiter:
                case TweenAnimation.TweenType.GlobalTimeScale: // don't animate global time scale in tests
                    continue;
            }
            var go = new GameObject(tweenType.ToString());
            var animationComponent = go.AddComponent<TweenAnimationComponent>(); // create as the first component on the GameObject for easier visual validation

            Object target = GetTarget();
            Object GetTarget() {
                if (tweenType == TweenAnimation.TweenType.MaterialPropertyVector4) {
                    return new Material(Shader.Find("ShaderWithVector4Property"));
                }
                if (targetType == typeof(Material)) {
                    return Resources.FindObjectsOfTypeAll<Material>().Single(x => x.name == "Default-Material");
                }
                if (targetType == typeof(Transform)) {
                    return go.GetComponent<Transform>();
                }
                if (targetType == typeof(TMP_Text)) {
                    return go.AddComponent<TextMeshPro>();
                }
                if (targetType == typeof(Graphic)) {
                    return go.AddComponent<RawImage>();
                }
                if (typeof(Object).IsAssignableFrom(targetType)) {
                    var result = go.AddComponent(targetType);
                    if (result is ScrollRect scrollRect) {
                        scrollRect.content = go.GetComponent<RectTransform>();
                        Assert.IsNotNull(scrollRect.content);
                    }
                    return result;
                }
                return null;
            }

            const float duration = 0.1f;
            var startValue = new Vector4(0.1f, 0.2f, 0.3f,0.4f).ToContainer();
            var endValue = new Vector4(0.4f, 0.3f, 0.2f,0.1f).ToContainer();
            var diff = endValue.vector4 - startValue.vector4;

            string stringParam = GetStringParam();
            string GetStringParam() {
                switch (tweenType) {
                    case TweenAnimation.TweenType.MaterialPropertyVector4:
                        return "_ColorVector";
                    case TweenAnimation.TweenType.MaterialAlphaProperty:
                    case TweenAnimation.TweenType.MaterialColorProperty:
                        return "_Color";
                    case TweenAnimation.TweenType.MaterialProperty:
                        return "_Mode";
                    case TweenAnimation.TweenType.MaterialTextureOffset:
                    case TweenAnimation.TweenType.MaterialTextureScale:
                        return "_MainTex";
                    default:
                        return string.Empty;
                }
            }

            var data = new TweenAnimation.Data {
                tweenType = tweenType,
                target = target,
                duration = duration,
                stringParam = stringParam,
                hasStartValue = TweenAnimation.Data.IsCustomTweenType(tweenType),
                startValue = startValue,
                endValue = endValue
            };
            bool isRegularShake = false;
            TweenAnimation.ValueWrapper customVal = default;
            switch (tweenType) {
                case TweenAnimation.TweenType.ShakeCamera:
                case TweenAnimation.TweenType.ShakeLocalPosition:
                case TweenAnimation.TweenType.ShakeLocalRotation:
                case TweenAnimation.TweenType.ShakeScale:
                    isRegularShake = true;
                    data.customData = new TweenAnimation.Data.Custom.Shake {
                        duration = duration,
                        strengthFactor = startValue.single,
                        strength = startValue.vector3,
                        frequency = 10f
                    };
                    break;
                case TweenAnimation.TweenType.ShakeCustom:
                    data.customData = new TweenAnimation.Data.Custom.ShakeCustom {
                        duration = duration,
                        startValue = startValue.vector3,
                        strength = startValue.vector3,
                        frequency = 10f,
                        unityEvent = new TweenAnimation.Data.Custom.Vector3.UnityEvent()
                    };
                    break;
                case TweenAnimation.TweenType.CustomColor: {
                    var evt = new TweenAnimation.Data.Custom.Color.UnityEvent();
                    evt.AddListener(x => customVal.color = x);
                    data.customData = new TweenAnimation.Data.Custom.Color { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomFloat: {
                    var evt = new TweenAnimation.Data.Custom.Float.UnityEvent();
                    evt.AddListener(x => customVal.single = x);
                    data.customData = new TweenAnimation.Data.Custom.Float { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomQuaternion: {
                    var evt = new TweenAnimation.Data.Custom.Quaternion.UnityEvent();
                    evt.AddListener(x => customVal.quaternion = x);
                    data.customData = new TweenAnimation.Data.Custom.Quaternion { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomRect: {
                    var evt = new TweenAnimation.Data.Custom.Rect.UnityEvent();
                    evt.AddListener(x => customVal.rect = x);
                    data.customData = new TweenAnimation.Data.Custom.Rect { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomVector2: {
                    var evt = new TweenAnimation.Data.Custom.Vector2.UnityEvent();
                    evt.AddListener(x => customVal.vector2 = x);
                    data.customData = new TweenAnimation.Data.Custom.Vector2 { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomVector3: {
                    var evt = new TweenAnimation.Data.Custom.Vector3.UnityEvent();
                    evt.AddListener(x => customVal.vector3 = x);
                    data.customData = new TweenAnimation.Data.Custom.Vector3 { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.CustomVector4: {
                    var evt = new TweenAnimation.Data.Custom.Vector4.UnityEvent();
                    evt.AddListener(x => customVal.vector4 = x);
                    data.customData = new TweenAnimation.Data.Custom.Vector4 { unityEvent = evt };
                    break;
                }
                case TweenAnimation.TweenType.Callback:
                    data.customData = new TweenAnimation.Data.Custom.Callback { unityEvent = new UnityEvent() };
                    break;
            }

            animationComponent.animation = new TweenAnimation {
                animations = new List<TweenAnimation.Data> { data },
                useUnscaledTime = tweenType == TweenAnimation.TweenType.GlobalTimeScale
            };

            if (testInspector) {
                Selection.activeGameObject = go;
                EditorWindow.GetWindow(inspectorWindowType);
                yield return null;
            }

            var seq = animationComponent.Play(true, true);
            Assert.IsTrue(seq.isAlive);
            ColdData t = null;
            foreach (var child in seq.GetAllChildren()) {
                t = child;
            }
            Assert.IsNotNull(t);

            switch (tweenType) {
                case TweenAnimation.TweenType.TweenAnimationComponent:
                case TweenAnimation.TweenType.Callback:
                    break;
                default:
                    Assert.AreEqual(duration, t.data.animationDuration);
                    break;
            }
            if (tweenType == TweenAnimation.TweenType.ShakeCamera) {
                Assert.AreEqual((target as Camera).GetComponent<Transform>(), t.managedData.target);
            } else if (tweenType != TweenAnimation.TweenType.TweenAnimationComponent) {
                if (targetType != null) {
                    Assert.AreEqual(target, t.managedData.target);
                }
                switch (tweenType) {
                    case TweenAnimation.TweenType.LocalRotation:
                        Assert.AreEqual(TweenAnimation.TweenType.LocalRotationQuaternion, t.data.tweenType);
                        break;
                    case TweenAnimation.TweenType.RigidbodyMoveRotation:
                        Assert.AreEqual(TweenAnimation.TweenType.RigidbodyMoveRotationQuaternion, t.data.tweenType);
                        break;
                    case TweenAnimation.TweenType.Rotation:
                        Assert.AreEqual(TweenAnimation.TweenType.RotationQuaternion, t.data.tweenType);
                        break;
                    case TweenAnimation.TweenType.ScaleUniform:
                        Assert.AreEqual(TweenAnimation.TweenType.Scale, t.data.tweenType);
                        break;
                    case TweenAnimation.TweenType.Callback:
                        Assert.AreEqual(TweenAnimation.TweenType.Delay, t.data.tweenType);
                        break;
                    default:
                        Assert.AreEqual(tweenType, t.data.tweenType);
                        break;
                }
                if (stringParam != string.Empty) {
                    Assert.AreEqual(t.intParam, Shader.PropertyToID(stringParam));
                }
                bool isTweenAnimationAppliesStartValues() {
                    return false;
                }
                if (isTweenAnimationAppliesStartValues()) {
                    Assert.AreEqual(false, t.data.startFromCurrent);
                } else {
                    switch (tweenType) {
                        case TweenAnimation.TweenType.ShakeLocalPosition:
                        case TweenAnimation.TweenType.ShakeLocalRotation:
                        case TweenAnimation.TweenType.ShakeScale:
                            Assert.AreEqual(true, t.data.startFromCurrent);
                            break;
                        default:
                            Assert.AreEqual(false, t.data.startFromCurrent);
                            break;
                    }
                }

                { // startValue, endValue
                    var tweenPropType = Utils.TweenTypeToTweenData(t.data.tweenType).Item1;
                    if (tweenPropType == PropType.Quaternion && propType == PropType.Vector3) {
                        Assert.AreEqual(Quaternion.Euler(startValue.vector3), t.data.startValue.quaternion);
                        Assert.AreEqual(Quaternion.Euler(endValue.vector3), t.managedData.endValueOrDiff.quaternion);
                    } else if (tweenType == TweenAnimation.TweenType.ScaleUniform) {
                        Assert.AreEqual(new Vector3(startValue.x, startValue.x, startValue.x), t.data.startValue.vector3);
                        Assert.AreEqual(new Vector3(diff.x, diff.x, diff.x), t.managedData.endValueOrDiff.vector3);
                    } else if (tweenType == TweenAnimation.TweenType.ShakeCustom) {
                        Assert.AreEqual(startValue.vector3, t.data.startValue.vector3);
                        Assert.AreEqual(-startValue.vector3, t.managedData.endValueOrDiff.vector3);
                    } else if (tweenType == TweenAnimation.TweenType.TextMaxVisibleCharacters) {
                        Assert.AreEqual(0f, t.data.startValue.x);
                        Assert.AreEqual(0f, t.managedData.endValueOrDiff.x);
                    } else if (isRegularShake) {
                        Vector3 expected = Vector3.zero;
                        if (isTweenAnimationAppliesStartValues() && tweenType == TweenAnimation.TweenType.ShakeScale) {
                            expected = Vector3.one;
                        }
                        Assert.AreEqual(expected, t.data.startValue.vector3);
                        Assert.AreEqual(Vector3.zero, t.managedData.endValueOrDiff.vector3);
                    } else if (tweenType == TweenAnimation.TweenType.Callback || tweenType == TweenAnimation.TweenType.Delay) {
                    } else if (tweenPropType == PropType.Quaternion) {
                        Assert.AreEqual(startValue.quaternion.normalized, t.data.startValue.quaternion);
                        Assert.AreEqual(endValue.quaternion.normalized, t.managedData.endValueOrDiff.quaternion);
                    } else {
                        Assert.AreEqual(propType, tweenPropType);
                        // startValue
                        Assert.AreEqual(startValue.x, t.data.startValue.x);
                        if (t.data.startValue.y != 0f) {
                            Assert.AreEqual(startValue.y, t.data.startValue.y);
                        }
                        if (t.data.startValue.z != 0f) {
                            Assert.AreEqual(startValue.z, t.data.startValue.z);
                        }
                        if (t.data.startValue.w != 0f) {
                            Assert.AreEqual(startValue.w, t.data.startValue.w);
                        }
                        // endValue
                        Assert.AreEqual(diff.x, t.managedData.endValueOrDiff.x);
                        if (t.managedData.endValueOrDiff.y != 0f) {
                            Assert.AreEqual(diff.y, t.managedData.endValueOrDiff.y);
                        }
                        if (t.managedData.endValueOrDiff.z != 0f) {
                            Assert.AreEqual(diff.z, t.managedData.endValueOrDiff.z);
                        }
                        if (t.managedData.endValueOrDiff.w != 0f) {
                            Assert.AreEqual(diff.w, t.managedData.endValueOrDiff.w);
                        }
                    }
                }
            }

            if (TweenAnimation.Data.IsCustomTweenType(tweenType)) {
                seq.progress = 1f;
                switch (propType) {
                    case PropType.Vector3:
                        Assert.AreEqual(customVal.vector3, TweenData.Vector3Val(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff));
                        break;
                    case PropType.Vector2:
                        Assert.AreEqual(customVal.vector2, TweenData.Vector2Val(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff));
                        break;
                    case  PropType.Color:
                        Assert.IsTrue(customVal.color == TweenData.ColorVal(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff));
                        break;
                    case PropType.Vector4:
                        Assert.IsTrue(customVal.vector4 == TweenData.Vector4Val(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff));
                        break;
                    case PropType.Float:
                        Assert.AreEqual(customVal.single, TweenData.FloatVal(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff));
                        break;
                    case PropType.Rect:
                        var rectVal = TweenData.RectVal(t.data.startValue, t.data.easedInterpolationFactor, t.managedData.endValueOrDiff);
                        UnityEngine.Assertions.Assert.AreApproximatelyEqual(customVal.rect.x, rectVal.x);
                        UnityEngine.Assertions.Assert.AreApproximatelyEqual(customVal.rect.y, rectVal.y);
                        UnityEngine.Assertions.Assert.AreApproximatelyEqual(customVal.rect.width, rectVal.width);
                        UnityEngine.Assertions.Assert.AreApproximatelyEqual(customVal.rect.height, rectVal.height);
                        break;
                    default: throw new Exception(tweenType.ToString());
                }
            }

            seq.Stop();
        }

        // EditorApplication.isPaused = true;
        // while (true) {
        //     yield return null;
        // }

        if (testInspector) {
            EditorWindow.GetWindow<TestRunnerWindow>();
        }
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TweenAnimationCoroutine() {
        TweenAnimation a = CreateTweenAnimation(Vector3.zero, Vector3.one, new Easing(), minDuration);
        a.Play();
        yield return a.ToYieldInstruction();
    }

    [Test]
    public async Task TweenAnimationAwait() {
        TweenAnimation a = CreateTweenAnimation(Vector3.zero, Vector3.one, new Easing(), minDuration);
        a.Play();
        await a;
    }

/*
TweenAnimationDirection (0.100s)
---
  (1.00, 1.00, 1.00), (1.00, 1.00, 1.00)
  Expected: True
  But was:  False

---
at Tests+<TweenAnimationDirection>d__21.MoveNext () [0x001b2] in src\com.kyrylokuzyk.primetween\Tests\Pro.cs:608
at UnityEngine.TestTools.TestEnumerator+<Execute>d__7.MoveNext () [0x0003a] in Library\PackageCache\com.unity.test-framework@1.4.5\UnityEngine.TestRunner\NUnitExtensions\Attributes\TestEnumerator.cs:44
*/
/*
TweenAnimationDirection (0.101s)
---
  (-1.00, -1.00, -1.00), (-1.00, -1.00, -1.00)
  Expected: True
  But was:  False

---
at Tests+<TweenAnimationDirection>d__21.MoveNext () [0x0022f] in src\com.kyrylokuzyk.primetween\Tests\Pro.cs:627
at UnityEngine.TestTools.TestEnumerator+<Execute>d__7.MoveNext () [0x0003a] in Library\PackageCache\com.unity.test-framework@1.4.5\UnityEngine.TestRunner\NUnitExtensions\Attributes\TestEnumerator.cs:44
*/
    [UnityTest]
    public IEnumerator TweenAnimationDirection() {
        var startValue = -Vector3.one;
        var endValue = Vector3.one;
        TweenAnimation a = CreateTweenAnimation(startValue, endValue, Ease.Linear, 10000f);
        a.cycleMode = Sequence.SequenceCycleMode.YoyoChildren;

        a.Play();
        Assert.IsFalse(a.isPaused);
        Assert.IsTrue(a.isAlive);
        Assert.IsTrue(a.isForward);

        a.Play(false);
        Assert.IsFalse(a.isForward);

        a.Play(false);
        Assert.IsFalse(a.isForward);

        a.Play(true);
        Assert.IsTrue(a.isForward);
        yield return null;
        Assert.IsTrue(startValue == transform.position, $"{startValue}, {transform.position}");

        a.Play(false);
        Assert.IsFalse(a.isForward);
        yield return null;
        Assert.IsTrue(endValue == transform.position, $"{endValue}, {transform.position}");

        a.Play(true);
        Assert.IsTrue(a.isForward);
        yield return null;
        Assert.IsTrue(startValue == transform.position, $"{startValue}, {transform.position}");

        a.Stop();
    }

    [Test]
    public void TweenAnimationDefaultProperties() {
        TweenAnimation a = CreateTweenAnimation(Vector3.zero, Vector3.one);
        _ = a.isAlive;
        a.Stop();
        a.Complete();
        expectDefaultCtorError();
        a.SetRemainingCycles(1);
        expectDefaultCtorError();
        _ = a.cyclesDone;
        expectDefaultCtorError();
        _ = a.cyclesTotal;
        expectDefaultCtorError();
        expectDefaultCtorError();
        a.isPaused = !a.isPaused;

        // expectDefaultCtorError();
        // expectDefaultCtorError();
        // a.timeScale = -a.timeScale;

        expectDefaultCtorError();
        _ = a.duration;
        expectDefaultCtorError();
        _ = a.durationTotal;
        expectDefaultCtorError();
        _ = a.elapsedTime;
        expectDefaultCtorError();
        _ = a.elapsedTimeTotal;
        expectDefaultCtorError();
        _ = a.progress;
        expectDefaultCtorError();
        _ = a.progressTotal;
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void TweenAnimationNesting() {
        TweenAnimationComponent nested = new GameObject(nameof(nested)).AddComponent<TweenAnimationComponent>();
        nested.animation = CreateTweenAnimation(Vector3.zero, Vector3.one);

        TweenAnimationComponent main = new GameObject(nameof(main)).AddComponent<TweenAnimationComponent>();
        main.animation = CreateTweenAnimation(Vector3.zero, Vector3.one);
        main.animation.animations = new List<TweenAnimation.Data> {
            main.animation.animations.Single(),
            new TweenAnimation.Data {
                target = nested,
                tweenType = TweenAnimation.TweenType.TweenAnimationComponent
            }
        };

        main.Play(null, true);
        Assert.IsTrue(nested.animation.isAlive);
        Assert.IsTrue(main.animation.isAlive);

        expectCantManipulateTweenInsideSequence();
        nested.animation.Stop();
    }

    [UnityTest]
    public IEnumerator TweenAnimationCycleModeRestart() {
        TweenAnimation a = CreateTweenAnimation(-Vector3.one, Vector3.one, Ease.Linear, 10000f);
        a.cycleMode = Sequence.SequenceCycleMode.Restart;

        a.onInterrupt = TweenAnimation.InterruptionMode.Rewind;
        {
            a.Play();
            Assert.IsTrue(a.isForward);

            a.Play();
            Assert.IsFalse(a.isForward);
            Assert.IsTrue(a.isAlive);
            yield return null;
            Assert.IsFalse(a.isAlive);

            a.Play(false);
            Assert.IsFalse(a.isAlive);

            var seq = a.PlayInternal(true);
            Assert.IsTrue(a.isAlive && a.isForward);

            a.Play(false);
            Assert.IsTrue(a.isAlive);
            Assert.IsFalse(a.isForward);
            Assert.AreEqual(seq, a._sequence);

            a.Complete();
            Assert.IsFalse(a.isForward);
        }

        a.onInterrupt = TweenAnimation.InterruptionMode.Restart;
        {
            var seq = a.PlayInternal();
            Assert.IsTrue(a.isAlive && a.isForward);
            a.Play();
            Assert.IsTrue(a.isAlive && a.isForward);
            Assert.AreNotEqual(seq, a._sequence);
        }
    }

    [Test]
    public void TweenAnimationCycleModeYoyoChildren() {
        const float duration = 2f;
        TweenAnimation a = CreateTweenAnimation(-Vector3.one, Vector3.one, Ease.Linear, duration);
        a.cycleMode = Sequence.SequenceCycleMode.YoyoChildren;

        void Test() {
            Assert.IsFalse(a.PlayInternal(false).isAlive);
            var seq = a.PlayInternal();
            Assert.IsTrue(a.isAlive && a.isForward);

            a.Play();
            Assert.IsTrue(a.isAlive && !a.isForward);
            Assert.AreEqual(seq, a._sequence);
            a.Complete();
        }

        a.onInterrupt = TweenAnimation.InterruptionMode.Rewind;
        a.cycles = 2;
        Test();

        a.cycles = 1;
        Test();

        a.onInterrupt = TweenAnimation.InterruptionMode.Restart;
        {
            var seq = a.PlayInternal();
            Assert.IsTrue(a.isAlive && a.isForward);
            Assert.AreNotEqual(seq, a.PlayInternal());
            Assert.IsTrue(a.isAlive && a.isForward);

            a.Play(false);
            a.Complete();
            Assert.IsFalse(a.isAlive);
            Assert.IsFalse(a.isForward);
        }

        a.cycles = 2;
        {
            Assert.IsFalse(a.PlayInternal(false).isAlive);
            a.PlayInternal(true).Complete();
            var seq = a.PlayInternal(false).root.tween;
            Assert.IsTrue(a.isAlive && !a.isForward);
            Assert.AreEqual(0f, seq.data.startValue.single);
            Assert.AreEqual(duration, seq.managedData.endValueOrDiff.single);
        }
    }

    [Test]
    public void TweenAnimationCycleModeRestartWithMultipleCycles() {
        var startValue = -Vector3.one;
        var endValue = Vector3.one;
        TweenAnimation a = CreateTweenAnimation(startValue, endValue, Ease.Linear, 10000f);
        a.cycleMode = Sequence.SequenceCycleMode.Restart;
        a.cycles = 2;

        a.Play(true);
        a.Complete();
        Assert.IsFalse(a.isAlive);
        Assert.AreEqual(endValue, transform.position);

        a.Play(false);
        a.Complete();
        Assert.IsFalse(a.isAlive);
        Assert.AreEqual(startValue, transform.position);
    }
}
#endif
