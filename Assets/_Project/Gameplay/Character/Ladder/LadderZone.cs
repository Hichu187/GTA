using UnityEngine;

namespace Game.Gameplay.Character.Ladder
{
    // Place on a trigger volume covering a climbable ladder. Unlike water (where walking out
    // horizontally naturally clears the trigger), climbing is vertical-only and gravity pulls
    // straight back down through the same column — so reaching the top/bottom can't rely on
    // passively drifting out of the trigger. ClimbState uses TopY/BottomY to explicitly step
    // the character off (see ClimbState.cs) instead.
    [RequireComponent(typeof(Collider))]
    public class LadderZone : MonoBehaviour
    {
        // X/Z position to snap the character onto while climbing (keeps them on the rail).
        public Vector3 MountPosition => transform.position;

        // Fixed facing direction the character holds while climbing this ladder — also the
        // direction ClimbState steps them off in when dismounting at the top or bottom.
        public Vector3 ClimbFacing => transform.forward;

        public float TopY    => GetComponent<Collider>().bounds.max.y;
        public float BottomY => GetComponent<Collider>().bounds.min.y;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}
