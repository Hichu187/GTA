namespace Game.Core.Camera
{
    public enum CameraBlendStyle
    {
        Cut         = 0,
        EaseInOut   = 1,
        Linear      = 2,
        SphericalLinear = 3,
    }

    public readonly struct CameraBlendSettings
    {
        public readonly float BlendTime;
        public readonly CameraBlendStyle BlendStyle;

        public CameraBlendSettings(float blendTime, CameraBlendStyle blendStyle = CameraBlendStyle.EaseInOut)
        {
            BlendTime   = blendTime;
            BlendStyle  = blendStyle;
        }

        public static readonly CameraBlendSettings Cut     = new CameraBlendSettings(0f,    CameraBlendStyle.Cut);
        public static readonly CameraBlendSettings Default = new CameraBlendSettings(0.5f,  CameraBlendStyle.EaseInOut);
    }
}
