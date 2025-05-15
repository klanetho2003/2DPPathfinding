using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

public class Creature : BaseController
{
    public CreatureData CreatureData { get; private set; }
    public CreatureMovementData CreatureMovementData { get; private set; }
    public MoveMentValues MovementValues { get; protected set; }

    public Vector2 RawMoveInput { get { return _moveDir; } }
    public float TargetVelocityX { get; protected set; }

    protected float _lastGroundedTime; // 마지막으로 지면에 닿은 시점 to Coyote

    protected Vector2 _moveDir = Vector2.zero;
    public Vector2 MoveDir
    {
        get { return _moveDir.normalized; }
        set
        {
            if (_moveDir == value) return;

            _moveDir = value;

            // Sprite 방향 전환
            if (value.x > 0)        LookRight = true;
            else if (value.x < 0)   LookRight = false;
        }
    }

    public bool OnWall { get { return OnRightWall | OnLeftWall; } }
    protected bool OnLeftWall = false;
    protected bool OnRightWall = false;

    protected bool IsGrounded = false;
    protected LayerMask _groundLayer;

    [SerializeField] // For Debug
    ECreatureState _creatureState;
    public ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState == value) return;
            OnStateChange(_creatureState, value);
            _creatureState = value;
            UpdateAnimation();
        }
    }

    #region Stat
    public float MaxHp { get; set; }
    public float MaxSpeed { get; set; }
    public float JumpForce { get; set; }

    private float _hp;
    public float Hp
    {
        get { return _hp; }
        set { _hp = Mathf.Clamp(value, 0, MaxHp); }
    }
    #endregion

    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<CapsuleCollider2D>();
        RigidBody = GetComponent<Rigidbody2D>();

        _groundLayer = LayerMask.GetMask("Ground");

        return true;
    }

    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        if (ObjectType == EObjectType.Player)
        {
            CreatureData = Managers.Data.PlayerDataDic[templateID];
            CreatureMovementData = Managers.Data.PlayerMovementDataDic[templateID];
        }
        else
        {
            // To Do Others
            CreatureData = Managers.Data.PlayerDataDic[templateID]; // Player랑 동일. Player랑 같은 Data Parsing
            CreatureMovementData = Managers.Data.EnemyMovementDataDic[templateID];
        }

        gameObject.name = $"{CreatureData.TemplateId}_{CreatureData.NameDataId}"; // To Do : string data sheet

        // Movement Data Load
        MovementValues = Managers.Resource.Load<MoveMentValues>($"MoveMentValues_{ObjectType}");

        // Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.size = new Vector2(CreatureData.ColliderSizeX, CreatureData.ColliderSizeY);

        // RigidBody
        RigidBody.mass = 0;

        // Animatior
        SetAnimation(CreatureData.AnimDataId, CreatureData.SortingLayerName, SortingLayers.CREATURE);

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = CreatureData.MaxHp;
        MaxSpeed = CreatureMovementData.MaxSpeed;
        JumpForce = CreatureMovementData.JumpForce;

        // State
        CreatureState = ECreatureState.Idle;
    }
    #endregion

    #region Update & State Method
    protected override void UpdateController()
    {
        // Grounded Check
        UpdateGrounded();

        // OnWall Check
        UpdateOnWall();

        // lastGroundedTime 갱신
        UpdateCoyoteTimer();

        switch (CreatureState)
        {
            case ECreatureState.Idle:   UpdateIdle(); break;
            case ECreatureState.Move:   UpdateMove(); break;
            case ECreatureState.Jump:   UpdateJump(); break;
            case ECreatureState.Fall:   UpdateFall(); break;
            case ECreatureState.Wall:   UpdateWall(); break;
            case ECreatureState.Dash:   UpdateDash(); break;
        }
    }

    protected override void FixedUpdateController()
    {
        // 목표 속도 계산
        CalculateTargetVelocity();

        // if -> 수평 이동 vs 마찰
        if (!Mathf.Approximately(RawMoveInput.x, 0f))
        {
            // 이동 방향이 설정_O -> 목표 속도로 설정
            SetRigidBodyVelocity(TargetVelocityX);
        }
        else if (IsGrounded)
        {
            // 이동 방향 설정_X && 지면에 있을 때 -> 지상 마찰만 적용
            ApplyGroundFriction();
        }
        // 공중에선 그대로 관성 유지
    }

    protected virtual void UpdateCoyoteTimer()
    {
        // 지면에 닿을 때마다 시간 리셋
        if (IsGrounded == false)
            return;

        _lastGroundedTime = Time.time;
    }

    protected virtual void UpdateIdle()
    {
        if (MoveDir != Vector2.zero) { CreatureState = ECreatureState.Move; return; }
    }
    protected virtual void UpdateMove()
    {
        if (MoveDir == Vector2.zero) { CreatureState = ECreatureState.Idle; return; }

        // 하강
        if (RigidBody.linearVelocityY < CreatureMovementData.MidToFallSpeedThreshold)
            CreatureState = ECreatureState.Fall;
    }
    protected virtual void UpdateJump() { }
    protected virtual void UpdateFall() { }
    protected virtual void UpdateWall() { }
    protected virtual void UpdateDash() { }

    protected override void UpdateAnimation() { }

    protected virtual void OnStateChange(ECreatureState brefore, ECreatureState after) { }
    #endregion

    #region Move Method
    protected virtual void UpdateGrounded()
    {
        IsGrounded = CheckGround();
    }

    protected virtual RaycastHit2D CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, _groundLayer);
        return hit;
    }

    private float _velocityXSmooth;
    private void CalculateTargetVelocity()
    {
        float accelTime = IsGrounded ? MovementValues.groundAccelTime : MovementValues.airAccelTime;
        float targetSpeed = RawMoveInput.x * MaxSpeed;
        TargetVelocityX = Mathf.SmoothDamp(RigidBody.linearVelocityX, targetSpeed, ref _velocityXSmooth, accelTime);
    }

    public virtual void SetRigidBodyVelocity(float velocity)
    {
        RigidBody.linearVelocity = new Vector2(velocity, RigidBody.linearVelocityY);
    }

    protected void ApplyGroundFriction()
    {
        if (!IsGrounded || !Mathf.Approximately(RawMoveInput.x, 0f))
            return;

        RigidBody.linearVelocity = new Vector2(
            RigidBody.linearVelocityX * MovementValues.groundFriction,
            RigidBody.linearVelocityY
        );
    }
    #endregion

    #region Jump Method
    protected virtual void DoJump(Vector2 dir, float force)
    {
        if (Mathf.Abs(dir.y) <= 0.1f  || force == 0) return;

        RigidBody.linearVelocity += dir * force;

        CreatureState = ECreatureState.Jump;
    }
    #endregion

    #region Wall Method

    Vector2 _onWallCheck_LineLeft_StartPos;
    Vector2 _onWallCheck_LineRight_StartPos;
    protected virtual void UpdateOnWall()
    {
        _onWallCheck_LineLeft_StartPos = (Vector2)transform.position + MovementValues.leftLineOffset;
        OnLeftWall = Physics2D.Raycast(_onWallCheck_LineLeft_StartPos, Vector2.left, 0.1f, _groundLayer);

        _onWallCheck_LineRight_StartPos = (Vector2)transform.position + MovementValues.rightLineOffset;
        OnRightWall = Physics2D.Raycast(_onWallCheck_LineRight_StartPos, Vector2.right, 0.1f, _groundLayer);
    }

    #endregion

    #region Helper
    protected bool IsGroundedWithCoyote()
    {
        return IsGrounded || (Time.time - _lastGroundedTime <= CreatureMovementData.CoyoteTimeDuration);
    }
    #endregion

#if UNITY_EDITOR
    // Gizmo to Debug
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // IsGrounded
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.down * 0.1f);

        // OnWall
        Gizmos.DrawLine(_onWallCheck_LineLeft_StartPos, _onWallCheck_LineLeft_StartPos + Vector2.left * 0.1f);
        Gizmos.DrawLine(_onWallCheck_LineRight_StartPos, _onWallCheck_LineRight_StartPos + Vector2.right * 0.1f);
    }
    #endif
}
