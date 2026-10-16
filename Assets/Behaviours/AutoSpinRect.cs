using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// RectTransform version of AutoSpin. Rotation is identical for RectTransform, so it reuses AutoSpin and only enforces a RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutoSpinRect : AutoSpin
    {
    }
}
