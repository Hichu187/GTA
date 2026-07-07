namespace Game.Core.Camera
{
    public interface ICameraContextProvider
    {
        CameraRigHandle     GetActiveCameraRig();
        CameraBlendSettings GetBlendSettings();
        event System.Action CameraRigChanged;
    }
}
