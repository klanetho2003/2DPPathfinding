using UnityEngine;

[CreateAssetMenu(fileName = "MoveMentValues", menuName = "Scriptable Objects/MoveMent")]
public class MoveMentValues : ScriptableObject
{
    [Header("Ground, MaxSpeed에 도달하는 데 걸리는 시간")]
    public float groundAccelTime = 0.1f;
    [Header("On Air, '' ")]
    public float airAccelTime = 0.2f;
    [Header("Frame당 감속량 On Ground -> 현재 속도 x Friction")]
    public float groundFriction = 1f;

    [Header("Fall 시, fallMultiplier – 1 만큼 떨어지는 속도에 곱해짐. (기본 중력 값이 1이어서 그만큼 빼고 연산함)")]
    public float fallMultiplier = 2.5f;
    [Header("Jump 중일 때, lowJumpMultiplier - 1만큼 상승 속도에 곱해짐 (기본 중력 값이 1이어서 그만큼 빼고 연산함")]
    public float lowJumpMultiplier = 2f;

    [Header("외쪽 벽 충돌 여부를 Check하는 Ray 시작 지점 자중치")]
    public Vector2 leftLineOffset = new Vector2(-0.5f, 0.5f);

    [Header("오른쪽 벽 '' ")]
    public Vector2 rightLineOffset = new Vector2(0.5f, 0.5f);

    [Header("벽에 닿아 있을 때 하강 속도")]
    public float wallSlideMaxSpeed = 2;
}
