using UnityEngine;

namespace PixelVanguard.Interfaces
{
    /// <summary>
    /// Interface for entities that can take damage.
    /// Provides events for damage feedback systems to subscribe to.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Event fired when this entity takes damage.
        /// Parameters: (damage amount, world position)
        /// </summary>
        event System.Action<float, Vector3> OnDamaged;
        
        /// <summary>
        /// Event fired when this entity is healed.
        /// Parameters: (heal amount, world position)
        /// </summary>
        event System.Action<float, Vector3> OnHealed;
        
        /// <summary>
        /// Whether this entity is currently alive.
        /// </summary>
        bool IsAlive { get; }
    }
}
