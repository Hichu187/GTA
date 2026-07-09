namespace Game.Core
{
    // Live per-frame state from a vehicle, queried every Update — must be cheap to read.
    public interface IVehicleRiderState
    {
        bool  IsMoving    { get; }  // true when vehicle has meaningful speed
        bool  TiltToRight { get; }  // true = leaning right (right foot touches ground when stopped)
        float SpeedNorm   { get; }  // 0 = stopped, 1 = top speed (for animator BikeSpeed param)
    }
}
