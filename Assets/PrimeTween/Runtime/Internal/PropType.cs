using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NamPhuThuy.AnimateWithScripts.PrimeTween.Editor")]
namespace PrimeTween {
    internal enum PropType : byte {
        None = 0,
        Float,
        Color,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
        Rect,
        Int,
        Double
    }
}
