using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    public static class ColorConst
    {
        // ========================================================================
        // PASTEL PALETTE (Soft, Eye-Friendly)
        // ========================================================================
    
        // Soft Pink / Red
        public static readonly Color PastelRed = new Color32(255, 179, 186, 255);
    
        // Soft Orange / Peach
        public static readonly Color PastelOrange = new Color32(255, 223, 186, 255);
    
        // Soft Yellow / Cream
        public static readonly Color PastelYellow = new Color32(255, 255, 186, 255);
    
        // Soft Green / Mint
        public static readonly Color PastelGreen = new Color32(186, 255, 201, 255);
    
        // Soft Blue / Sky
        public static readonly Color PastelBlue = new Color32(186, 225, 255, 255);
        
        // Soft Cyan / Aqua
        public static readonly Color PastelCyan = new Color32(179, 255, 255, 255);
    
        // Soft Purple / Lavender
        public static readonly Color PastelPurple = new Color32(223, 186, 255, 255);

        // ========================================================================
        // UTILITY COLORS
        // ========================================================================
    
        public static readonly Color Transparent = new Color(0, 0, 0, 0);
        public static readonly Color WhiteAlpha50 = new Color(1, 1, 1, 0.5f);
        public static readonly Color BlackAlpha50 = new Color(0, 0, 0, 0.5f);

        
    }
}