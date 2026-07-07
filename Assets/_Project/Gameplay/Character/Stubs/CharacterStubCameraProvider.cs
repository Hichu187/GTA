using UnityEngine;
using Game.Core.Camera;

namespace Game.Gameplay.Character.Stubs
{
    public class CharacterStubCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        public event System.Action CameraRigChanged;

        public CameraRigHandle    GetActiveCameraRig()  => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => CameraBlendSettings.Default;
    }
}
