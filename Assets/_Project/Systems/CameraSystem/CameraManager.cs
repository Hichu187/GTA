using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Systems.Camera
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain _brain;

        private readonly List<CinemachineCamera> _registeredCameras = new();

        private void Awake()
        {
            if (_brain == null)
                _brain = UnityEngine.Camera.main?.GetComponent<CinemachineBrain>();
        }

        // Called by scene bootstrapper to register VirtualCameras so this manager can deactivate them.
        public void RegisterCamera(CinemachineCamera vcam)
        {
            if (!_registeredCameras.Contains(vcam))
                _registeredCameras.Add(vcam);
        }

        // Called by GameBootstrapper on scene start to seed the registry before first Possess.
        public void FindAndRegisterAllCameras()
        {
            var found = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var vcam in found)
                RegisterCamera(vcam);

            foreach (var vcam in _registeredCameras)
                vcam.Priority = 0;
        }

        // Called by PossessionManager after each Possess.
        public void ApplyContext(ICameraContextProvider provider)
        {
            var handle = provider.GetActiveCameraRig();
            if (!handle.IsValid) return;

            var target = handle.CameraGameObject.GetComponent<CinemachineCamera>();
            if (target == null) return;

            SetBlend(provider.GetBlendSettings());
            ActivateOnly(target);
        }

        private void ActivateOnly(CinemachineCamera target)
        {
            foreach (var vcam in _registeredCameras)
                vcam.Priority = 0;

            target.Priority = 10;

            if (!_registeredCameras.Contains(target))
                _registeredCameras.Add(target);
        }

        private void SetBlend(CameraBlendSettings settings)
        {
            if (_brain == null) return;
            _brain.DefaultBlend = new CinemachineBlendDefinition(MapStyle(settings.BlendStyle), settings.BlendTime);
        }

        private static CinemachineBlendDefinition.Styles MapStyle(CameraBlendStyle style) => style switch
        {
            CameraBlendStyle.Cut    => CinemachineBlendDefinition.Styles.Cut,
            CameraBlendStyle.Linear => CinemachineBlendDefinition.Styles.Linear,
            _                       => CinemachineBlendDefinition.Styles.EaseInOut,
        };
    }
}
