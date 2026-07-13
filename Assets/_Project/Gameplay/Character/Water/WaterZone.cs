using UnityEngine;

namespace Game.Gameplay.Character.Water
{
    // Place on a trigger volume covering a body of water. Layer should be the
    // "Water" physics layer (already reserved in TagManager, previously unused).
    [RequireComponent(typeof(Collider))]
    public class WaterZone : MonoBehaviour
    {
        [Tooltip("World-space Y of the water surface. Defaults to this transform's position.")]
        [SerializeField] private bool  _useTransformY = true;
        [SerializeField] private float _surfaceY;

        public float SurfaceY => _useTransformY ? transform.position.y : _surfaceY;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}
