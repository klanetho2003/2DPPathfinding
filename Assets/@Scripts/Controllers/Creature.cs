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

        // State
        CreatureState = ECreatureState.Idle;

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = CreatureData.MaxHp;
        MaxSpeed = CreatureData.MaxSpeed;
        JumpForce = CreatureData.JumpForce;
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
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateJump() { }
    protected virtual void UpdateMove() { }

    protected override void UpdateAnimation() { }
    #endregion

    #region FixedUpdate & Move Method
    protected override void FixedUpdateController()
    {
        // 속도 적용
        SetRigidBodyVelocity(TargetVelocityX);

        // 마찰력 부여
        ApplyGroundFriction();


        // 좌표 연산
        /*Vector3 dest = MoveDir.normalized * MoveSpeed;
        transform.position = transform.position + (dest * Time.fixedDeltaTime);*/
    }

    protected virtual void UpdateGrounded()
    {
        IsGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));
    }

    private float velocityXSmoothing; // 속도 보간 진행 저장용
    private void CalculateTargetVelocity()
    {
        float accelTime = IsGrounded ? MovementValues.groundAccelTime : MovementValues.airAccelTime;
        float targetSpeed = MoveDir.x * MaxSpeed;
        TargetVelocityX = Mathf.SmoothDamp(RigidBody.linearVelocityX, targetSpeed, ref velocityXSmoothing, accelTime);
    }

    public virtual void SetRigidBodyVelocity(float velocity)
    {
        if (CreatureState != ECreatureState.Move)
            return;

        RigidBody.linearVelocity = new Vector2(velocity, RigidBody.linearVelocityY);

        if (velocity < 0)
            LookRight = false;
        else if (velocity > 0)
            LookRight = true;
    }

    protected void ApplyGroundFriction()
    {
        if (CreatureState == ECreatureState.Move)
            return;
        if (!IsGrounded || Mathf.Approximately(MoveDir.x, 0f) == false)
            return;

        RigidBody.linearVelocity = new Vector2(RigidBody.linearVelocityX * MovementValues.groundFriction, RigidBody.linearVelocityY);
    }
    #endregion
}
