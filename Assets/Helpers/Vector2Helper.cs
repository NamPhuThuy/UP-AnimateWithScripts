using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    
    public static class Vector2Helper
    {
        /// <summary>
        /// Returns a random Vector2 position within a circle of the specified radius.
        /// </summary>
        /// <param name="radius">Radius of the circle.</param>
        /// <returns>Random position as Vector2.</returns>
        public static Vector2 RandomPointInCircle(float radius)
        {
            float angle = Random.Range(0f, 360f);
            float r = Random.Range(0f, radius);
            float x = r * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = r * Mathf.Sin(angle * Mathf.Deg2Rad);
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// Returns a random Vector2 position on the circumference of a circle with the specified radius, optionally biased towards a direction.
        /// </summary>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="biasAngle">Preferred angle in degrees (0 = right, 90 = up, etc.).</param>
        /// <param name="biasStrength">Strength of bias (0 = uniform, 1 = always biased).</param>
        /// <param name="preferHorizontal">If true, biases towards left (180°) or right (0°) randomly.</param>
        /// <returns>Random position as Vector2.</returns>
        public static Vector2 RandomPointInRange(float radius, float biasAngle = 0f, float biasStrength = 0f, bool preferHorizontal = false)
        {
            float angle = Random.Range(0f, 360f);
            if (preferHorizontal)
            {
                biasAngle = Random.value > 0.5f ? 0f : 180f; // Randomly choose right or left
                biasStrength = Mathf.Max(biasStrength, 0.8f); // Ensure strong bias
            }
            angle = Mathf.Lerp(angle, biasAngle, biasStrength);
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            return new Vector2(x, y);
        }

    }
}