/*
https://github.com/NamPhuThuy
 */


using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public static class BezierCurveHelper
    {
        // Easing delegate
        public delegate float Ease(float t);

        // Common easings
        public static float Linear(float t) => t;
        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        // --- Evaluate methods (sampling only) ---
        public static Vector3 EvaluateQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return (u * u) * p0 + 2f * u * t * p1 + (t * t) * p2;
        }

        public static Vector3 EvaluateCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;
            return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        }

        // --- Quadratic animation: explicit control point ---
        public static Coroutine AnimateQuadratic(
            MonoBehaviour runner,
            Transform subject,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            float duration,
            Ease ease = null,
            bool rotate = false,
            Vector3 rotateEuler = default,      // e.g., new Vector3(0, 0, 360f)
            bool restoreRotation = true,
            bool useLocalSpace = false,
            Action onStart = null,
            Action<float> onStep = null,
            Action onComplete = null)
        {
            if (runner == null || subject == null)
                return null;

            return runner.StartCoroutine(IE_AnimateQuadratic(
                subject, p0, p1, p2, duration,
                ease ?? EaseOutCubic,
                rotate, rotateEuler, restoreRotation, useLocalSpace,
                onStart, onStep, onComplete));
        }

        // --- Quadratic animation: auto control point from arc height and up ---
        public static Coroutine AnimateQuadraticArc(
            MonoBehaviour runner,
            Transform subject,
            Vector3 start,
            Vector3 end,
            float duration,
            float arcHeight = 0f,
            Vector3 up = default,               // defaults to Vector3.up
            Ease ease = null,
            bool rotate = false,
            Vector3 rotateEuler = default,
            bool restoreRotation = true,
            bool useLocalSpace = false,
            Action onStart = null,
            Action<float> onStep = null,
            Action onComplete = null)
        {
            if (runner == null || subject == null)
                return null;

            if (up == default) up = Vector3.up;
            Vector3 mid = (start + end) * 0.5f;
            Vector3 p1 = mid + up.normalized * arcHeight;

            return AnimateQuadratic(
                runner, subject, start, p1, end, duration,
                ease, rotate, rotateEuler, restoreRotation, useLocalSpace,
                onStart, onStep, onComplete);
        }

        // --- Cubic animation: two control points ---
        public static Coroutine AnimateCubic(
            MonoBehaviour runner,
            Transform subject,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float duration,
            Ease ease = null,
            bool rotate = false,
            Vector3 rotateEuler = default,
            bool restoreRotation = true,
            bool useLocalSpace = false,
            Action onStart = null,
            Action<float> onStep = null,
            Action onComplete = null)
        {
            if (runner == null || subject == null)
                return null;

            return runner.StartCoroutine(IE_AnimateCubic(
                subject, p0, p1, p2, p3, duration,
                ease ?? EaseOutCubic,
                rotate, rotateEuler, restoreRotation, useLocalSpace,
                onStart, onStep, onComplete));
        }

        // --- Coroutines ---
        private static IEnumerator IE_AnimateQuadratic(
            Transform subject,
            Vector3 p0, Vector3 p1, Vector3 p2,
            float duration,
            Ease ease,
            bool rotate,
            Vector3 rotateEuler,
            bool restoreRotation,
            bool useLocalSpace,
            Action onStart,
            Action<float> onStep,
            Action onComplete)
        {
            if (duration <= 0f)
            {
                SetPosition(subject, p2, useLocalSpace);
                onStart?.Invoke();
                onStep?.Invoke(1f);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            Vector3 startEuler = subject.eulerAngles;

            onStart?.Invoke();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float et = ease != null ? Mathf.Clamp01(ease(t)) : t;

                Vector3 pos = EvaluateQuadratic(p0, p1, p2, et);
                SetPosition(subject, pos, useLocalSpace);

                if (rotate)
                {
                    subject.eulerAngles = startEuler + rotateEuler * et;
                }

                onStep?.Invoke(et);
                yield return null;
            }

            SetPosition(subject, p2, useLocalSpace);
            if (restoreRotation) subject.eulerAngles = startEuler;

            onComplete?.Invoke();
        }

        private static IEnumerator IE_AnimateCubic(
            Transform subject,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            float duration,
            Ease ease,
            bool rotate,
            Vector3 rotateEuler,
            bool restoreRotation,
            bool useLocalSpace,
            Action onStart,
            Action<float> onStep,
            Action onComplete)
        {
            if (duration <= 0f)
            {
                SetPosition(subject, p3, useLocalSpace);
                onStart?.Invoke();
                onStep?.Invoke(1f);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            Vector3 startEuler = subject.eulerAngles;

            onStart?.Invoke();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float et = ease != null ? Mathf.Clamp01(ease(t)) : t;

                Vector3 pos = EvaluateCubic(p0, p1, p2, p3, et);
                SetPosition(subject, pos, useLocalSpace);

                if (rotate)
                {
                    subject.eulerAngles = startEuler + rotateEuler * et;
                }

                onStep?.Invoke(et);
                yield return null;
            }

            SetPosition(subject, p3, useLocalSpace);
            if (restoreRotation) subject.eulerAngles = startEuler;

            onComplete?.Invoke();
        }

        private static void SetPosition(Transform tr, Vector3 pos, bool local)
        {
            if (local) tr.localPosition = pos;
            else tr.position = pos;
        }
    }
    
    
}
