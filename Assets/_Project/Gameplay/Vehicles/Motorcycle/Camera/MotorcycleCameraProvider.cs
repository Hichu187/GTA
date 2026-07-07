using UnityEngine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    public class MotorcycleCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        public event System.Action CameraRigChanged;

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.4f, CameraBlendStyle.EaseInOut);
    }
}
