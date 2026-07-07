using UnityEngine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    public class DummyVehicleCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        public event System.Action CameraRigChanged;

        public CameraRigHandle    GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()  => new CameraBlendSettings(0.3f, CameraBlendStyle.EaseInOut);
    }
}
