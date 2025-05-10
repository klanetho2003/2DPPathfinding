using UnityEngine;

[CreateAssetMenu(fileName = "MoveMentValues", menuName = "Scriptable Objects/MoveMent")]
public class MoveMentValues : ScriptableObject
{
    [Header("MaxSpeed에 도달하는 데 걸리는 시간 on Ground")]
    public float groundAccelTime = 0.1f;
    [Header("MaxSpeed에 도달하는 데 걸리는 시간 on Air")]
    public float airAccelTime = 0.2f;
    [Header("Frame당 감속량 On Ground -> 현재 속도 x Friction")]
    public float groundFriction = 1f;
}
