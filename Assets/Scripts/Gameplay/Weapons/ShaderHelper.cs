using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Utility class for common shader operations in weapons.
    /// Centralizes material instance creation and shader property setup.
    /// </summary>
    public static class ShaderHelper
    {
        /// <summary>
        /// Creates an instance material with reveal shader property for visual effects.
        /// </summary>
        /// <param name="renderer">SpriteRenderer to apply the material to</param>
        /// <returns>Tuple containing the material instance and reveal property ID</returns>
        public static (Material material, int revealPropID) CreateRevealMaterial(SpriteRenderer renderer)
        {
            var material = new Material(renderer.material);
            renderer.material = material;
            int propID = Shader.PropertyToID("_RevealAmount");
            return (material, propID);
        }
    }
}
