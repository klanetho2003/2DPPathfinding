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

    [Header("Fall 시, fallMultiplier – 1 만큼 떨어지는 속도에 곱해짐. (기본 중력 값이 1이어서 그만큼 빼고 연산함)")]
    public float fallMultiplier = 2.5f;
    [Header("Jump 중일 때, lowJumpMultiplier - 1만큼 상승 속도에 곱해짐 (기본 중력 값이 1이어서 그만큼 빼고 연산함")]
    public float lowJumpMultiplier = 2f;
}
