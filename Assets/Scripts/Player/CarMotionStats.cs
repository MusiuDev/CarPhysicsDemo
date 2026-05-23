using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarMotionStats", menuName = "Custom Scriptables/CarMotionStats")]
public class CarMotionStats : ScriptableObject
{
    public float MaxSpeed;
    public float Acceleration;
    public float SteerTorque;
    public float MaxAngularSpeed;
    public float DriftRecoverySpeed;
    public float FastDriftRecoveryThreshold;
    public float FastDriftRecoveryMult;
    public float TractionMutliplier;
    public float AirControl;
    public float BaseDamping;
    public float StoppedDamping;
    public float DriftingDamping;
    public float BaseAngularDamping;
    public float SteeringAngularDamping;
    public float StraightAngularDamping;
}

/// <summary>
/// A registry of the CarMotionStats' values mapped to an enum. This allows values to be overriden by status effects and car stat settings without having to override the whole value set. 
/// </summary>
public static class CarStatsRegistry
{
    public static readonly Dictionary<CarStatType, (Func<CarMotionStats, float> Get, Action<CarMotionStats, float> Set)> Fields = new()
    {
        {CarStatType.MaxSpeed, (so=>so.MaxSpeed,(so,value)=>so.MaxSpeed = value)},
        {CarStatType.Acceleration, (so=>so.Acceleration,(so,value)=>so.Acceleration = value)},
        {CarStatType.SteerTorque, (so=>so.SteerTorque,(so,value)=>so.SteerTorque = value)},
        {CarStatType.MaxAngularSpeed, (so=>so.MaxAngularSpeed,(so,value)=>so.MaxAngularSpeed = value)},
        {CarStatType.DriftRecoverySpeed, (so=>so.DriftRecoverySpeed,(so,value)=>so.DriftRecoverySpeed = value)},
        {CarStatType.FastDriftRecoveryThreshold, (so=>so.FastDriftRecoveryThreshold,(so,value)=>so.FastDriftRecoveryThreshold = value)},
        {CarStatType.FastDriftRecoveryMult, (so=>so.FastDriftRecoveryMult,(so,value)=>so.FastDriftRecoveryMult = value)},
        {CarStatType.TractionMutliplier, (so=>so.TractionMutliplier,(so,value)=>so.TractionMutliplier = value)},
        {CarStatType.AirControl, (so=>so.AirControl,(so,value)=>so.AirControl = value)},
        {CarStatType.BaseDamping, (so=>so.BaseDamping,(so,value)=>so.BaseDamping = value)},
        {CarStatType.StoppedDamping, (so=>so.StoppedDamping,(so,value)=>so.StoppedDamping = value)},
        {CarStatType.DriftingDamping, (so=>so.DriftingDamping,(so,value)=>so.DriftingDamping = value)},
        {CarStatType.BaseAngularDamping, (so=>so.BaseAngularDamping,(so,value)=>so.BaseAngularDamping = value)},
        {CarStatType.SteeringAngularDamping, (so=>so.SteeringAngularDamping,(so,value)=>so.SteeringAngularDamping = value)},
        {CarStatType.StraightAngularDamping, (so=>so.StraightAngularDamping,(so,value)=>so.StraightAngularDamping = value)},
    };
}

public enum CarStatType
{
    MaxSpeed,
    Acceleration,
    SteerTorque,
    MaxAngularSpeed,
    DriftRecoverySpeed,
    FastDriftRecoveryThreshold,
    FastDriftRecoveryMult,
    TractionMutliplier,
    AirControl,
    BaseDamping,
    StoppedDamping,
    DriftingDamping,
    BaseAngularDamping,
    SteeringAngularDamping,
    StraightAngularDamping
}

public enum StatOverrideOperation
{
    Set,
    Mult
}

[System.Serializable]
public struct CarStatOverride
{
    public CarStatType type;
    public StatOverrideOperation style;
    public float value;
}
