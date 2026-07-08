using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Game.Editor
{
    public static class CharacterAnimatorSetup
    {
        private const string k_SavePath = "Assets/_Project/Gameplay/Character/Animation/CharacterLocomotion.controller";

        // Speed thresholds (normalised to SprintSpeed=9):
        //   Idle=0, Walk=3/9≈0.333, Run=5/9≈0.556, Sprint=9/9=1.0
        //   CrouchIdle=0, CrouchWalk=2/9≈0.222
        private static readonly float[] k_GroundThresholds = { 0f, 0.333f, 0.556f, 1f };
        private static readonly float[] k_CrouchThresholds = { 0f, 0.222f };

        [MenuItem("Game/Character/Create Animator Controller")]
        public static void Create()
        {
            EnsureFolder();

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(k_SavePath);

            AddParameters(ctrl);

            var sm = ctrl.layers[0].stateMachine;
            sm.entryPosition  = new Vector3(-200f, 0f);
            sm.anyStatePosition = new Vector3(-200f, 80f);
            sm.exitPosition   = new Vector3(-200f, 160f);

            // ── Blend tree states ──────────────────────────────────────────
            var groundState = MakeBlendTree(ctrl, "GroundLocomotion", k_GroundThresholds);
            var crouchState = MakeBlendTree(ctrl, "CrouchLocomotion", k_CrouchThresholds);

            // ── One-shot states ────────────────────────────────────────────
            var jumpState = sm.AddState("Jump", new Vector3(380f,  80f));
            var fallState = sm.AddState("Fall", new Vector3(380f, 180f));
            var landState = sm.AddState("Land", new Vector3(380f, 280f));

            sm.defaultState = groundState;

            // ── Transitions: GroundLocomotion ──────────────────────────────
            // → Jump  (trigger, no exit time, instant)
            T(groundState, jumpState,
                exitTime: false, duration: 0.05f,
                ("Jump", AnimatorConditionMode.If, 0f));

            // → Fall  (leaves ground without jumping)
            T(groundState, fallState,
                exitTime: false, duration: 0.1f,
                ("IsGrounded", AnimatorConditionMode.IfNot, 0f));

            // → Crouch
            T(groundState, crouchState,
                exitTime: false, duration: 0.1f,
                ("IsCrouching", AnimatorConditionMode.If, 0f));

            // ── Transitions: CrouchLocomotion ──────────────────────────────
            T(crouchState, groundState,
                exitTime: false, duration: 0.1f,
                ("IsCrouching", AnimatorConditionMode.IfNot, 0f));

            T(crouchState, jumpState,
                exitTime: false, duration: 0.05f,
                ("Jump", AnimatorConditionMode.If, 0f));

            T(crouchState, fallState,
                exitTime: false, duration: 0.1f,
                ("IsGrounded", AnimatorConditionMode.IfNot, 0f));

            // ── Transitions: Jump → Fall (after most of clip plays) ────────
            var jf = jumpState.AddTransition(fallState);
            jf.hasExitTime      = true;
            jf.exitTime         = 0.9f;
            jf.duration         = 0.1f;
            jf.hasFixedDuration = true;

            // ── Transitions: Fall → Land ───────────────────────────────────
            T(fallState, landState,
                exitTime: false, duration: 0.05f,
                ("IsGrounded", AnimatorConditionMode.If, 0f));

            // ── Transitions: Land → GroundLocomotion (play clip fully) ─────
            var lg = landState.AddTransition(groundState);
            lg.hasExitTime      = true;
            lg.exitTime         = 1f;
            lg.duration         = 0.05f;
            lg.hasFixedDuration = true;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = ctrl;

            Debug.Log("[GTA] CharacterLocomotion.controller created at " + k_SavePath
                + "\nAssign animation clips to the blend tree slots in the Inspector.");
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static void AddParameters(AnimatorController ctrl)
        {
            ctrl.AddParameter("Speed",       AnimatorControllerParameterType.Float);
            ctrl.AddParameter("MoveX",       AnimatorControllerParameterType.Float);
            ctrl.AddParameter("MoveY",       AnimatorControllerParameterType.Float);
            ctrl.AddParameter("IsGrounded",  AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Jump",        AnimatorControllerParameterType.Trigger);
        }

        private static AnimatorState MakeBlendTree(
            AnimatorController ctrl,
            string stateName,
            float[] thresholds)
        {
            BlendTree tree;
            var state = ctrl.CreateBlendTreeInController(stateName, out tree, 0);
            tree.blendType                = BlendTreeType.Simple1D;
            tree.blendParameter           = "Speed";
            tree.useAutomaticThresholds   = false;

            foreach (float threshold in thresholds)
                tree.AddChild(null, threshold);

            return state;
        }

        // Transition with arbitrary conditions; duration is fixed (seconds).
        private static void T(
            AnimatorState from, AnimatorState to,
            bool exitTime, float duration,
            params (string name, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            var t = from.AddTransition(to);
            t.hasExitTime      = exitTime;
            t.exitTime         = 0f;
            t.duration         = duration;
            t.hasFixedDuration = true;
            t.offset           = 0f;
            foreach (var (name, mode, threshold) in conditions)
                t.AddCondition(mode, threshold, name);
        }

        private static void EnsureFolder()
        {
            const string parent = "Assets/_Project/Gameplay/Character";
            const string child  = "Animation";
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
