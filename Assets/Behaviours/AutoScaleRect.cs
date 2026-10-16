using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// RectTransform version of AutoScale. Scale is identical for RectTransform, so it reuses AutoScale and only enforces a RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutoScaleRect : AutoScale
    {
    }
}
