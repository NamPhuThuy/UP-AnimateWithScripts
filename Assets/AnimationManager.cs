/*
Github: https://github.com/NamPhuThuy/UP-AnimateWithScripts
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    [DefaultExecutionOrder(-50)]
    public class AnimationManager : Singleton<AnimationManager>
    {
        [Header("Components")]
        [SerializeField] private AnimationCatalog animationCatalog;
        
        // type -> pooled objects
        private readonly Dictionary<AnimationType, Queue<AnimationBase>> _pool = new();
        private readonly Dictionary<AnimationBase, AnimationType> _reverse = new();
        // type -> active playing objects (in order of activation, oldest first)
        private readonly Dictionary<AnimationType, List<AnimationBase>> _active = new();
        
        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();
            PreloadAnimations();
            
            /*DebugLogger.Log(message:$"anchored posi: {GetComponent<RectTransform>().anchoredPosition}");
            DebugLogger.Log(message:$"rect position: {GetComponent<RectTransform>().position}");
            DebugLogger.Log(message:$"transform position: {transform.position}");*/
        }

        #endregion

        void PreloadAnimations()
        {
            if (!animationCatalog) return;
            foreach (var e in animationCatalog.entries)
                Preload(e.type, e.preload);
        }

        public void Preload(AnimationType type, int count)
        {
            var entry = animationCatalog.GetEntry(type);
            if (entry == null || !entry.prefab) return;

            if (!_pool.ContainsKey(type)) _pool[type] = new Queue<AnimationBase>();
            var q = _pool[type];

            while (q.Count < count)
            {
                var go = Instantiate(entry.prefab, transform);
                go.gameObject.SetActive(false);
                go.IsInPool = true;
                q.Enqueue(go);
                
                _reverse[go] = type;
            }
        }

        private AnimationBase Get(AnimationType type)
        {
            if (!_pool.TryGetValue(type, out var poolQueue))
            {
                poolQueue = new Queue<AnimationBase>();
                _pool[type] = poolQueue;
            }

            if (!_active.TryGetValue(type, out var activeList))
            {
                activeList = new List<AnimationBase>();
                _active[type] = activeList;
            }

            // Clean up any destroyed references without delegate allocation
            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                if (activeList[i] == null)
                    activeList.RemoveAt(i);
            }

            var entry = animationCatalog.GetEntry(type);
            if (entry == null || !entry.prefab)
            {
                DebugLogger.LogError($"Missing VFX prefab for {type}", context: this); 
                return null;
            }

            // 1. If we have an idle instance in the pool, use it
            if (poolQueue.Count > 0)
            {
                var pooledInst = poolQueue.Dequeue();
                pooledInst.IsInPool = false;
                activeList.Add(pooledInst);
                return pooledInst;
            }

            int limit = entry.limit > 0 ? entry.limit : 5;

            // 2. If active limit reached, recycle and reuse the earliest active animation
            if (activeList.Count >= limit && activeList.Count > 0)
            {
                var earliest = activeList[0];
                activeList.RemoveAt(0);
                earliest.EndFast();

                AnimationBase inst = (poolQueue.Count > 0) ? poolQueue.Dequeue() : earliest;
                inst.IsInPool = false;
                activeList.Add(inst);
                return inst;
            }

            // 3. Otherwise instantiate a new instance up to the limit
            var newInst = Instantiate(entry.prefab, transform);
            newInst.IsInPool = false;
            _reverse[newInst] = type;
            activeList.Add(newInst);
            return newInst;
        }
        
        public void Release(AnimationBase animation)
        {
            if (!animation || animation.IsInPool) return;
            if (!_reverse.TryGetValue(animation, out var type)) return;

            animation.IsInPool = true;

            if (_active.TryGetValue(type, out var activeList))
            {
                activeList.Remove(animation);
            }

            animation.transform.SetParent(transform, false);
            animation.gameObject.SetActive(false);

            if (!_pool.TryGetValue(type, out var poolQueue))
            {
                poolQueue = new Queue<AnimationBase>();
                _pool[type] = poolQueue;
            }

            poolQueue.Enqueue(animation);
        }


        #region Public Methods
        
        public AnimationBase Play<T>(T args) where T : struct, IAnimationArgs
        {
            AnimationBase animationBase = Get(args.Type);
            if (!animationBase) return null;

            // Play with type-safe arguments
            animationBase.Play(args);
            
            return animationBase;
        }

        #endregion

        #region Default Effects Calls

        public void PlayBasicPopupText(string message, float duration = 0f)
        {
            var args = new ToastArgs
            {
                message = message,
                customAnchoredPos = AnimationConst.UPPER_ANCHORED_POS,
                textColor = Color.white,
                customDuration = duration,
            };
            Play(args);
        }

        #endregion
    }

}

/*
// 1) Coins fly to panel; update the counter when they ARRIVE:
var coinPanel = GUIManager.Ins.GUIShop.CoinPanel; // your panel Transform
var ticker = coinPanel.GetComponentInChildren<NumberTicker>();

int delta = 250;
VFXManager.Ins.PlayAt(
    VFXType.COIN_FLY,
    pos: someWorldPoint,
    amount: delta,
    target: coinPanel.transform,
    onArrive: () => ticker?.AnimateDelta(delta)
);

// 2) Simple popup text in world:
VFXManager.Ins.PlayAt(
    VFXType.POPUP_TEXT,
    pos: worldPos,
    message: "+3 Moves"
);

// 3) Just a particle burst:
VFXManager.Ins.PlayAt(VFXType.HIT_SPARK, pos: hitPoint);
 */