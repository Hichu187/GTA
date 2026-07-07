using UnityEngine;

namespace Game.Core.Camera
{
    // CameraManager (Game.Systems.Camera) receives this handle and does
    // GetComponent<CinemachineVirtualCameraBase>() itself — keeping Core
    // free of the Cinemachine package dependency.
    public readonly struct CameraRigHandle
    {
        public readonly GameObject CameraGameObject;
        public readonly int Priority;

        public CameraRigHandle(GameObject cameraGameObject, int priority = 10)
        {
            CameraGameObject = cameraGameObject;
            Priority         = priority;
        }

        public bool IsValid => CameraGameObject != null;
    }
}
