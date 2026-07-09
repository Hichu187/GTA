using UnityEngine;

namespace Game.Core
{
    public class VehicleRiderData
    {
        public bool      HideCharacter;

        // Foot pegs / pedals — where feet rest while riding
        public Transform LeftFootTarget;
        public Transform RightFootTarget;

        // Ground stand positions — where feet touch the ground when the bike is stopped
        public Transform LeftStandTarget;
        public Transform RightStandTarget;

        // Handlebar grips
        public Transform LeftHandTarget;
        public Transform RightHandTarget;

        // IK Hints — guide the solver for knee/elbow bend direction.
        // Optional: null = solver picks direction from base animation.
        public Transform LeftKneeHint;
        public Transform RightKneeHint;
        public Transform LeftElbowHint;
        public Transform RightElbowHint;

        // Spine look target — body bends toward this point while riding.
        // Optional: if null, midpoint of LeftHandTarget + RightHandTarget is used.
        public Transform SpineLookTarget;

        // Hip anchor — forces the rider's hips to this world position so arms can reach the handlebar.
        // Place at pelvis height on the seat. If null, body position is left to animation.
        public Transform SeatAnchor;

        // Live state provider — queried every frame for IsMoving / TiltToRight.
        // Null if the vehicle doesn't support stand behaviour.
        public IVehicleRiderState StateSource;
    }
}
