using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Enemy : Creature
{
    public virtual Player Target { get; private set; }

    private List<Vector3Int> _pathCells = new();
    private int _pathIndex = 0;

    private float _pathUpdateInterval = 0.5f;
    private float _pathUpdateTimer = 0f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Enemy;

        // To Do -> Temp
        MovementValues = Managers.Resource.Load<MoveMentValues>($"MoveMentValues_{EObjectType.Player}");

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

            // To Do
            /*case ECreatureState.Jump:
                {
                    if (RigidBody.linearVelocityY > PlayerMovementData.JumpToMidSpeedThreshold)
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
                break;*/
        }
    }
    #endregion

    protected override void UpdateController()
    {
        if (Target.IsValid() == false)
            return;

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        if (MoveDir != Vector2.zero) { CreatureState = ECreatureState.Move; return; }
    }

    protected override void UpdateMove()
    {
        if (MoveDir == Vector2.zero) { CreatureState = ECreatureState.Idle; return; }
    }

    protected override void FixedUpdateController()
    {
        if (Target == null)
            return;

        base.FixedUpdateController();        

        _pathUpdateTimer += Time.fixedDeltaTime;
        if (_pathUpdateTimer >= _pathUpdateInterval)
        {
            _pathUpdateTimer = 0f;
            UpdatePath(); // 일정 주기마다 경로 갱신
        }

        MoveAlongPath();
    }

    private void UpdatePath()
    {
        Vector3Int start = Managers.Map.World2Cell(transform.position);
        Vector3Int dest = Managers.Map.World2Cell(Target.transform.position);

        _pathCells = Managers.Map.FindPathSideView(this, start, dest, maxDepth: 20);
        _pathIndex = 0;

        if (_pathCells.Count > 0)
            CellPos = _pathCells[_pathIndex];
    }

    protected void MoveAlongPath()
    {
        if (_pathCells == null || _pathCells.Count == 0)
            return;

        Vector3Int currentGroundCell = Managers.Map.World2Cell(transform.position); // 현재 실제 위치

        // 현재 위치가 다음 목적 셀에 도달 or 지나쳤는지 체크
        if (currentGroundCell == _pathCells[_pathIndex])
        {
            _pathIndex++;
            if (_pathIndex >= _pathCells.Count)
            {
                _pathCells.Clear();
                MoveDir = Vector3.zero;
                return;
            }

            CellPos = _pathCells[_pathIndex];
        }

        // 다음 셀 기준으로 MoveDir 설정 (X 방향만 사용)
        Vector3 dest = Managers.Map.Cell2World(CellPos);
        Vector3 dir = dest - transform.position;
        MoveDir = new Vector3(Mathf.Sign(dir.x), 0, 0);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_pathCells == null || _pathCells.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < _pathCells.Count - 1; i++)
        {
            Vector3 from = Managers.Map.Cell2World(_pathCells[i]);
            Vector3 to = Managers.Map.Cell2World(_pathCells[i + 1]);
            Gizmos.DrawLine(from, to);
            Gizmos.DrawSphere(from, 0.1f);
        }
    }
    #endif
}
