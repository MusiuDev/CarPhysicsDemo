using UnityEngine;

public class CarState : ICarState
{
    public float Speed { get; set; }
    public float SteerInput { get; set; }
    public bool AccelerateInput { get; set; }
    public bool BrakeInput { get; set; }
    public float ContactingMult { get; set; }
    public int ContactingWheels { get; set; }
    public float TotalSlopeTraction { get; set; }
    public float DriftingFactor { get; set; }
    public int IntentionAngle { get; set; }
    public float CurrentSpeedFactor { get; set; }
    public Vector3 RbForward { get; set; }
    public Vector3 RbUp { get; set; }
    public float MotionAngle { get; set; }

    public void ResetMotion()
    {
        SteerInput = default;
        AccelerateInput = default;
        BrakeInput = default;
        ContactingMult = default;
        ContactingWheels = default;
        TotalSlopeTraction = default;
        DriftingFactor = default;
        IntentionAngle = default;
        CurrentSpeedFactor = default;
        RbForward = default;
        RbUp = default;
        MotionAngle = default;
    }
}

public interface ICarState
{
    float Speed { get; }
    float SteerInput { get; }
    bool AccelerateInput { get; }
    bool BrakeInput { get; }
    float ContactingMult { get; }
    int ContactingWheels { get; }
    float TotalSlopeTraction { get; }
    float DriftingFactor { get; }
    int IntentionAngle { get; }
    float CurrentSpeedFactor { get; }
    Vector3 RbForward { get; }
    Vector3 RbUp { get; }
    float MotionAngle { get; }
}