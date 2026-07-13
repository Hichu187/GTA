using UnityEngine;

namespace Game.Gameplay.Character.Water
{
    // Tracks whether the character's CharacterController capsule is deep enough in a
    // WaterZone to swim. Relies on Unity's built-in behavior where a CharacterController
    // receives OnTriggerEnter/Exit from other trigger colliders it moves into — no
    // Rigidbody or manual overlap polling needed.
    [RequireComponent(typeof(CharacterController))]
    public class CharacterWaterDetector : MonoBehaviour
    {
        [Tooltip("Fraction of the capsule height that must be underwater before swimming kicks in (0-1). Shallower water is still walkable.")]
        [SerializeField] private float _swimDepthFraction = 0.6f;

        private CharacterController _controller;
        private WaterZone            _currentZone;

        public bool  IsInWater       { get; private set; }
        public float SubmersionDepth { get; private set; }
        public float SurfaceY        { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var zone = other.GetComponent<WaterZone>();
            if (zone != null) _currentZone = zone;
        }

        private void OnTriggerExit(Collider other)
        {
            var zone = other.GetComponent<WaterZone>();
            if (zone != null && zone == _currentZone) _currentZone = null;
        }

        private void Update()
        {
            if (_currentZone == null)
            {
                IsInWater       = false;
                SubmersionDepth = 0f;
                return;
            }

            SurfaceY = _currentZone.SurfaceY;

            float feetY = transform.position.y + _controller.center.y - _controller.height * 0.5f;
            float headY = feetY + _controller.height;

            SubmersionDepth = Mathf.Max(0f, SurfaceY - headY);
            IsInWater        = SurfaceY >= feetY + _controller.height * _swimDepthFraction;
        }
    }
}
