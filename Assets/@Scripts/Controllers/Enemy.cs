using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

public class Enemy : Creature
{
    public virtual Player Target { get; private set; }

    public EnemyMovementData EnemyMovementData { get { return (EnemyMovementData)CreatureMovementData; } }

    private List<Vector3Int> _pathCells = new();
    private int _pathIndex = 0;
    private float _pathUpdateTimer = 0f;

    private bool _shouldJump = false;
    private Vector2 _jumpDir = Vector2.zero;
    private float _jumpPower = 0f;
    Vector3Int _stepPos = Vector3Int.zero;

    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Enemy;

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        Target = Managers.Object.Player;

        // 충돌 기피 객체 정의
        LayerMask excludeMask = 0;
        excludeMask.AddLayer(ELayer.Creature);
        Collider.excludeLayers = excludeMask;
    }
    #endregion

    #region Update
    protected override void UpdateController()
    {
        if (Target.IsValid() == false)
            return;

        base.UpdateController();
    }

    protected override void FixedUpdateController()
    {
        if (Target == null)
            return;

        base.FixedUpdateController();

        _pathUpdateTimer += Time.fixedDeltaTime;
        if (_pathUpdateTimer >= EnemyMovementData.PathUpdateInterval)
        {
            _pathUpdateTimer = 0f;
            UpdatePath();
        }

        // 점프 필요 여부 확인 및 값 세팅
        MoveAlongPath();

        // 점프 실행 조건 최종 확인
        if (_shouldJump && _pathCells != null && _pathIndex < _pathCells.Count)
        {
            Vector3Int jumpTargetCell = _pathCells[_pathIndex];

            if (jumpTargetCell.y > _stepPos.y)
            {
                DoJump(_jumpDir, _jumpPower);
                ClearJumpReservation();
            }
        }
        else
        {
            // 경로가 끝났거나 유효하지 않으면 예약 제거
            ClearJumpReservation();
        }
    }
    #endregion

    #region Animation
    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                Anim.Play("Idle");
                break;
            case ECreatureState.Move:
                Anim.Play("Run");
                break;
            case ECreatureState.Jump:
                {
                    if (RigidBody.linearVelocityY > EnemyMovementData.JumpToMidSpeedThreshold)
                        Anim.Play("JumpRise");
                    else
                        Anim.Play("JumpMid");
                }
                break;
            case ECreatureState.Fall:
                Anim.Play("JumpFall");
                break;
            case ECreatureState.Wall:
                Anim.Play("WallClimbIdle");
                break;
            case ECreatureState.Dash:
                Anim.Play("DashLoop");
                break;
        }
    }
    #endregion

    #region State Pattern
    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        // 하강
        if (RigidBody.linearVelocityY < EnemyMovementData.MidToFallSpeedThreshold)
            CreatureState = ECreatureState.Fall;
    }

    protected override void UpdateJump()
    {
        // 하강
        if (RigidBody.linearVelocityY < EnemyMovementData.MidToFallSpeedThreshold)
            CreatureState = ECreatureState.Fall;

        // 착지
        else if (IsGrounded && Util.IsEqualValue(RigidBody.linearVelocityY, 0))
            CreatureState = ECreatureState.Idle;

        // Jump Mid 구간으로 Animation을 바꾸기 위함
        if (CreatureState == ECreatureState.Jump)
            UpdateAnimation();
    }

    protected override void UpdateFall()
    {
        if (IsGrounded)
            CreatureState = ECreatureState.Idle;
    }
    #endregion

    #region Jump
    protected override void DoJump(Vector2 dir, float force)
    {
        if (dir == Vector2.zero || force <= 0f)
            return;

        // 점프 파워를 절대 속도로 환산
        float jumpVelocity = Mathf.Sqrt(1.8f * force * Mathf.Abs(Physics2D.gravity.y));

        // 점프 방향 보정 (X는 부드럽게, Y는 고정 상향)
        Vector2 jumpVec = new Vector2(dir.x * 0.4f, 1f).normalized * jumpVelocity;

        // 기존 수직 속도 제거 후 직접 세팅
        RigidBody.linearVelocity = new Vector2(jumpVec.x, jumpVec.y);

        CreatureState = ECreatureState.Jump;
    }
    #endregion

    #region Find Path
    private void UpdatePath()
    {
        Vector3Int start = Managers.Map.World2Cell(transform.position);
        Vector3Int dest = Managers.Map.World2Cell(Target.transform.position);

        _pathCells = Managers.Map.FindPath(this, start, dest, maxDepth: 20);
        _pathIndex = 0;

        if (_pathCells.Count > 0)
            CellPos = _pathCells[_pathIndex];
    }

    private void MoveAlongPath()
    {
        if (_pathCells == null || _pathCells.Count == 0)
            return;

        // 현재 self 위치 정의
        RaycastHit2D hit = CheckGround();
        Vector2 stepPos = (hit == true) ? hit.point : transform.position;
        _stepPos = Managers.Map.World2Cell(stepPos);

        // 최종 목적지에 도착 -> 빠른 탈출
        if (_pathIndex >= _pathCells.Count)
        {
            _pathCells.Clear();
            MoveDir = Vector3.zero;
            return;
        }

        // Node 이동 완료, Next Path 정의
        if (_stepPos == _pathCells[_pathIndex])
        {
            _pathIndex++;
            // Check
            if (_pathIndex >= _pathCells.Count)
            {
                _pathCells.Clear();
                MoveDir = Vector3.zero;
                return;
            }

            // Next Path Setting
            CellPos = _pathCells[_pathIndex];
        }

        Vector3 dest = Managers.Map.Cell2World(CellPos);
        Vector3 dir = dest - transform.position;
        MoveDir = new Vector3(Mathf.Sign(dir.x), 0, 0);

        // 점프 간선 확인 → 점프 예약
        if (Managers.Map.TryGetEdge(_stepPos, _pathCells[_pathIndex], out var edge) &&
            edge.edgeType == EdgeType.Jump &&
            CreatureMovementData.JumpForce >= edge.cost)
        {
            // 예약만 (방향 + 점프력 저장)
            _shouldJump = true;
            _jumpPower = edge.cost;

            // 방향 계산 (목표 타일 기준)
            Vector3 worldTarget = Managers.Map.Cell2World(_pathCells[_pathIndex]);
            _jumpDir = (worldTarget - transform.position).normalized;
        }
    }
    private void ClearJumpReservation()
    {
        _shouldJump = false;
        _jumpDir = Vector2.zero;
        _jumpPower = 0f;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_pathCells == null) return;

        Gizmos.color = Color.red;
        foreach (var cell in _pathCells)
        {
            Vector3 world = Managers.Map.Cell2World(cell);
            Gizmos.DrawWireCube(world, Vector3.one * 0.5f);
        }
    }
    #endif
}
