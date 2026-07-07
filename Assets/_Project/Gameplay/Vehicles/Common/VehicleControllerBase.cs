using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;

namespace Game.Gameplay.Vehicles.Common
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class VehicleControllerBase : MonoBehaviour, IPossessable
    {
        [SerializeField] protected Transform _enterAnchor;
        [SerializeField] protected Transform _exitAnchor;

        protected Rigidbody   _rb;
        protected System.Action _onExitRequested;

        public Transform EnterAnchor => _enterAnchor;
        public Transform ExitAnchor  => _exitAnchor;
        public bool      IsOccupied  { get; private set; }

        public abstract ICameraContextProvider  CameraProvider { get; }
        public abstract IHUDContextProvider     HUDProvider    { get; }
        public abstract IInputActionMapProvider InputProvider  { get; }

        protected virtual void Awake() => _rb = GetComponent<Rigidbody>();

        public virtual void OnPossess(PossessionContext context)
        {
            IsOccupied       = true;
            _onExitRequested = context.OnExitRequested;
        }

        public virtual void OnUnpossess(PossessionContext context)
        {
            IsOccupied       = false;
            _onExitRequested = null;
        }

        private void Update()
        {
            if (IsOccupied) OnOccupiedUpdate();
        }

        private void FixedUpdate()
        {
            if (IsOccupied) OnOccupiedFixedUpdate();
        }

        /// <summary>Override to handle per-frame input (non-physics).</summary>
        protected virtual void OnOccupiedUpdate() { }

        /// <summary>Override to handle physics — called in FixedUpdate while possessed.</summary>
        protected virtual void OnOccupiedFixedUpdate() { }
    }
}
