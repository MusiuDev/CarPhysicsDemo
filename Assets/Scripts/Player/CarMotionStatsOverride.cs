using UnityEngine;

[CreateAssetMenu(fileName = "CarMotionStatsOverride", menuName = "Custom Scriptables/CarMotionStatsOverride")]
public class CarMotionStatsOverride : ScriptableObject
{
    public CarStatOverride[] overrides;
}
