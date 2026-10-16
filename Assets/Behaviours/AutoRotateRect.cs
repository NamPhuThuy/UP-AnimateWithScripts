using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    /// <summary>
    /// RectTransform version of AutoRotate. Rotation is identical for RectTransform, so it reuses AutoRotate and only enforces a RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutoRotateRect : AutoRotate
    {
    }
}
