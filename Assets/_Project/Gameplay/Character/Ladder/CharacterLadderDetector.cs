using UnityEngine;

namespace Game.Gameplay.Character.Ladder
{
    // Tracks whether the character is currently inside a LadderZone trigger. Relies on the
    // same CharacterController-generates-trigger-events behavior as CharacterWaterDetector
    // — no Rigidbody or manual overlap polling needed.
    [RequireComponent(typeof(CharacterController))]
    public class CharacterLadderDetector : MonoBehaviour
    {
        private LadderZone _currentZone;

        public bool    IsOnLadder    { get; private set; }
        public Vector3 MountPosition { get; private set; }
        public Vector3 ClimbFacing   { get; private set; }
        public float   TopY          { get; private set; }
        public float   BottomY       { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            var zone = other.GetComponent<LadderZone>();
            if (zone != null) _currentZone = zone;
        }

        private void OnTriggerExit(Collider other)
        {
            var zone = other.GetComponent<LadderZone>();
            if (zone != null && zone == _currentZone) _currentZone = null;
        }

        private void Update()
        {
            IsOnLadder = _currentZone != null;
            if (IsOnLadder)
            {
                MountPosition = _currentZone.MountPosition;
                ClimbFacing   = _currentZone.ClimbFacing;
                TopY          = _currentZone.TopY;
                BottomY       = _currentZone.BottomY;
            }
        }
    }
}
