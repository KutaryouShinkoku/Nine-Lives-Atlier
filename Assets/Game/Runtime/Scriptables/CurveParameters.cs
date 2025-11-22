using UnityEngine;

[CreateAssetMenu(fileName = "CurveParameters", menuName = "Hand Curve Parameters")]
public class CurveParameters : ScriptableObject
{
    [Header("Positioning Parameters")]
    public AnimationCurve positioning;
    public float positioningInfluence = 0.1f;
}
