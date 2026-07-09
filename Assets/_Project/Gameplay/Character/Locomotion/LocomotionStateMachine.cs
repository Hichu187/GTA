using System.Collections.Generic;
using UnityEngine;
using Game.Gameplay.Character.Locomotion.States;

namespace Game.Gameplay.Character.Locomotion
{
    public class LocomotionStateMachine
    {
        private readonly Dictionary<LocomotionStateId, ILocomotionState> _states;
        private ILocomotionState _current;

        public LocomotionStateId CurrentId { get; private set; }

        public LocomotionStateMachine()
        {
            _states = new Dictionary<LocomotionStateId, ILocomotionState>
            {
                [LocomotionStateId.Idle]   = new IdleState(),
                [LocomotionStateId.Walk]   = new WalkState(),
                [LocomotionStateId.Run]    = new RunState(),
                [LocomotionStateId.Sprint] = new SprintState(),
                [LocomotionStateId.Jump]   = new JumpState(),
                [LocomotionStateId.Fall]   = new FallState(),
                [LocomotionStateId.Land]   = new LandState(),
                [LocomotionStateId.Crouch] = new CrouchState(),
            };
        }

        public void Start(LocomotionContext ctx) => Transition(LocomotionStateId.Idle, ctx);

        public void Tick(LocomotionContext ctx)
        {
            ctx.StateTimer += Time.deltaTime;

            if (ctx.IsGrounded)
                ctx.GroundGraceTimer = ctx.Config.SlopeGraceDuration;
            else
                ctx.GroundGraceTimer = Mathf.Max(0f, ctx.GroundGraceTimer - Time.deltaTime);

            var next = _current.Update(ctx);
            if (next != LocomotionStateId.Self && next != CurrentId)
                Transition(next, ctx);
        }

        private void Transition(LocomotionStateId id, LocomotionContext ctx)
        {
            _current?.Exit(ctx);
            CurrentId = id;
            _current = _states[id];
            ctx.StateTimer = 0f;
            _current.Enter(ctx);
        }
    }
}
