using Data;
using UnityEngine;
using static Define;

public class Creature : BaseController
{
    public CreatureData CreatureData { get; private set; }

    public MoveMentValues MovementValues { get; protected set; }

    public float TargetVelocityX { get; protected set; }

    protected Vector2 _moveDir = Vector2.zero;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value.normalized;

            UpdateAnimation(); // To Do
        }
    }

    [SerializeField] // For Debug
    protected bool _isGrounded = false;
    public bool IsGrounded
    {
        get { return _isGrounded; }
        protected set { _isGrounded = value; }
    }

    [SerializeField] // For Debug
    ECreatureState _creatureState;
    public ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState == value)
                return;

            _creatureState = value;
        }
    }

    #region Stat
    float _hp;
    public float Hp
    {
        get { return _hp; }
        set { _hp = Mathf.Clamp(value, 0, MaxHp);}
    }
    public float MaxHp { get; set; }
    public float MaxSpeed { get; set; }
    public float JumpForce { get; set; }
    #endregion

    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<CapsuleCollider2D>();
        RigidBody = GetComponent<Rigidbody2D>();

        return true;
    }

    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        if (ObjectType == EObjectType.Player)
        {
            CreatureData = Managers.Data.PlayerDataDic[templateID];
        }
        else
        {
            // To Do Others
        }

        gameObject.name = $"{CreatureData.TemplateId}_{CreatureData.NameDataId}"; // To Do : string data sheet

        // Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.size = new Vector2(CreatureData.ColliderSizeX, CreatureData.ColliderSizeY);

        // RigidBody
        RigidBody.mass = 0;

        // Material
        // SpriteRenderer.material = Managers.Resource.Load<Material>(CreatureData.MaterialID);

        // Animatior
        SetAnimation(CreatureData.AnimDataId, CreatureData.SortingLayerName, SortingLayers.CREATURE);

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = CreatureData.MaxHp;
        MaxSpeed = CreatureData.MaxSpeed;
        JumpForce = CreatureData.JumpForce;

        // State
        CreatureState = ECreatureState.Idle;
    }
    #endregion

    #region Update
    protected override void UpdateController()
    {
        // Grounded Check
        UpdateGrounded();

        // 목표 속도 계산
        CalculateTargetVelocity();

        switch (CreatureState)
        {
            case ECreatureState.Idle:
                UpdateIdle();
                break;
            case ECreatureState.Move:
                UpdateMove();
                break;
            case ECreatureState.Jump:
                UpdateJump();
                break;
            case ECreatureState.Fall:
                UpdateFall();
                break;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateJump() { }
    protected virtual void UpdateFall() { }

    protected override void UpdateAnimation() { }
    #endregion

    #region FixedUpdate & Move Method
    protected override void FixedUpdateController()
    {
        // if -> 수평 이동 vs 마찰
        if (!Mathf.Approximately(MoveDir.x, 0f))
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

        // 2. Better Jump: 강화 낙하 & 가변 점프 높이
        ApplyBetterJump();
    }

    protected virtual void UpdateGrounded()
    {
        IsGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
    }

    private float velocityXSmoothing;
    private void CalculateTargetVelocity()
    {
        float accelTime = IsGrounded ? MovementValues.groundAccelTime : MovementValues.airAccelTime;
        float targetSpeed = MoveDir.x * MaxSpeed;
        TargetVelocityX = Mathf.SmoothDamp(RigidBody.linearVelocityX, targetSpeed, ref velocityXSmoothing, accelTime);
    }

    public virtual void SetRigidBodyVelocity(float velocity)
    {
        RigidBody.linearVelocity = new Vector2(velocity, RigidBody.linearVelocityY);

        // Animation 방향 전환
        LookRight = TargetVelocityX > 0;
    }

    protected void ApplyGroundFriction()
    {
        if (!IsGrounded || !Mathf.Approximately(MoveDir.x, 0f))
            return;

        RigidBody.linearVelocity = new Vector2(
            RigidBody.linearVelocityX * MovementValues.groundFriction,
            RigidBody.linearVelocityY
        );
    }

    protected virtual void DoJump(Vector2 dir, bool onWall)
    {
        if (!IsGrounded) return;

        RigidBody.linearVelocity += dir * JumpForce;

        CreatureState = ECreatureState.Jump;
    }

    protected virtual void ApplyBetterJump()
    {
        if (IsGrounded)
            return;

        float vy = RigidBody.linearVelocityY;
        float multiplier = 1f;

        // 빠른 낙하
        if (vy < 0f)
            multiplier = MovementValues.fallMultiplier;

        // 적용
        RigidBody.linearVelocity = Vector2.up * Physics2D.gravity.y * (multiplier - 1f) * Time.fixedDeltaTime;
    }
    #endregion
}
