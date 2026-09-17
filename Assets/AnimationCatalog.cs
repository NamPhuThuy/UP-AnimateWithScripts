/*
Github: https://github.com/NamPhuThuy/UP-AnimateWithScripts
*/


using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public enum AnimationType
    {
        NONE = 0,
        TOAST = 1,
        TOAST_WORLD_SPACE = 2,
        TOAST_IMAGE = 3,
        SPRITE_MOTION = 8,
        ITEM_FLY = 11,
    }

    [CreateAssetMenu(fileName = "AnimationCatalog", menuName = "NamPhuThuy_AnimateWithScripts/Animation Catalog")]
    public class AnimationCatalog : ScriptableObject
    {
        public List<Entry> entries = new();
        
        [NonSerialized]
        private Dictionary<AnimationType, Entry> _dictTypeEntry;

        public Dictionary<AnimationType, Entry> DictTypeEntry
        {
            get
            {
                if (_dictTypeEntry == null)
                    EnsureDict();
                return _dictTypeEntry;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        // private void OnValidate() => InvalidateIndex();
#endif

        #endregion
        
        #region Private Methods

        private void EnsureDict()
        {
            if (_dictTypeEntry != null) return;

            _dictTypeEntry = new Dictionary<AnimationType, Entry>(entries?.Count ?? 0);
            if (entries == null) return;

            // Last write wins if duplicates exist
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                _dictTypeEntry[e.type] = e;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Force the index to rebuild on next use.</summary>
        public void InvalidateIndex() => _dictTypeEntry = null;
        
        /// <summary>Get entry by type (null if missing).</summary>
        public Entry GetEntry(AnimationType type)
        {
            EnsureDict();
            if (_dictTypeEntry == null) return null;
            _dictTypeEntry.TryGetValue(type, out var e);
            return e;
        }

        /// <summary>Try-get pattern to avoid null checks.</summary>
        public bool TryGetEntry(AnimationType type, out Entry entry)
        {
            EnsureDict();
            if (_dictTypeEntry == null)
            {
                entry = null;
                return false;
            }
            return _dictTypeEntry.TryGetValue(type, out entry);
        }
        
        /// <summary>Does an entry exist for this type?</summary>
        public bool IsContains(AnimationType type)
        {
            EnsureDict();
            return _dictTypeEntry != null && _dictTypeEntry.ContainsKey(type);
        }
        
        /// <summary>Get the prefab for a VFX type (null if missing).</summary>
        public AnimationBase GetPrefab(AnimationType type)
        {
            return GetEntry(type)?.prefab;
        }
        
        /// <summary>Get preload count (fallback if missing).</summary>
        public int GetPreload(AnimationType type, int fallback = 0)
        {
            var e = GetEntry(type);
            return e != null ? Mathf.Max(0, e.preload) : Mathf.Max(0, fallback);
        }

        /// <summary>Get safety auto-release seconds (fallback if missing).</summary>
        public float GetSoftMaxAliveSeconds(AnimationType type, float fallback = 10f)
        {
            var e = GetEntry(type);
            return e != null ? Mathf.Max(0f, e.softMaxAliveSeconds) : Mathf.Max(0f, fallback);
        }

        /// <summary>Get max limit count (fallback if missing or 0).</summary>
        public int GetLimit(AnimationType type, int fallback = 5)
        {
            var e = GetEntry(type);
            return (e != null && e.limit > 0) ? e.limit : Mathf.Max(1, fallback);
        }

        #endregion
        
        [Serializable]
        public class Entry
        {
            public AnimationType type;
            public AnimationBase prefab; 
            [Min(0)] public int preload = 3;
            [Min(1)] public int limit = 5;
            public float softMaxAliveSeconds = 10f; // safety auto-release
        }
        
    }
}