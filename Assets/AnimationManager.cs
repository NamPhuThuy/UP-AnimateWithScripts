/*
Github: https://github.com/NamPhuThuy
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
        
        [SerializeField] private TMP_FontAsset defaultFont;
        public TMP_FontAsset DefaultFont => defaultFont;
        
        // type -> pooled objects
        private readonly Dictionary<AnimationType, Queue<AnimationBase>> _pool = new();
        private readonly Dictionary<AnimationBase, AnimationType> _reverse = new();
        
        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();
            PreloadAll();
            
            /*DebugLogger.Log(message:$"anchored posi: {GetComponent<RectTransform>().anchoredPosition}");
            DebugLogger.Log(message:$"rect position: {GetComponent<RectTransform>().position}");
            DebugLogger.Log(message:$"transform position: {transform.position}");*/
        }

        #endregion

        void PreloadAll()
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
                q.Enqueue(go);
                
                _reverse[go] = type;
            }
        }

        private AnimationBase Get(AnimationType type)
        {
            if (!_pool.TryGetValue(type, out var q)) { q = new Queue<AnimationBase>(); _pool[type] = q; }
            if (q.Count > 0) return q.Dequeue();

            var entry = animationCatalog.GetEntry(type);
            if (entry == null || !entry.prefab)
            {
                DebugLogger.LogError($"Missing VFX prefab for {type}", context: this); 
                return null;
            }

            var inst = Instantiate(entry.prefab, transform);
            _reverse[inst] = type;
            return inst;
        }
        
        public void Release(AnimationBase animation)
        {
            if (!animation) return;
            if (!_reverse.TryGetValue(animation, out var type)) return;

            animation.transform.SetParent(transform, false);
            animation.gameObject.SetActive(false);
            _pool[type].Enqueue(animation);
        }


        #region Public Methods
        
        public T Play<T>(T args) where T : struct, IAnimationArgs
        {
            AnimationBase animationBase = Get(args.Type);
            if (!animationBase) return args;

            // Play with type-safe arguments
            animationBase.Play(args);
            
            return args;
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