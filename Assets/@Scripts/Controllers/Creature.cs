using Data;
using UnityEngine;
using static Define;

public class Creature : BaseController
{
    public CreatureData CreatureData { get; private set; }

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
    public float MoveSpeed { get; set; }
    #endregion

    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;

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
        MoveSpeed = CreatureData.MoveSpeed;
    }
    #endregion

    #region Update
    protected override void UpdateController()
    {
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
    protected virtual void UpdateMove() { }
    protected virtual void UpdateJump() { }

    protected override void UpdateAnimation() { }
    #endregion

    #region Move
    protected override void FixedUpdateController()
    {
        if (CreatureState != ECreatureState.Move)
        {
            SetRigidBodyVelocity(Vector3.zero);
            return;
        }

        // 좌표 연산 -> 적용
        Vector3 dest = MoveDir.normalized * MoveSpeed;
        transform.position = transform.position + (dest * Time.fixedDeltaTime);

        SetRigidBodyVelocity(dest);
    }

    public virtual void SetRigidBodyVelocity(Vector2 velocity)
    {
        if (RigidBody == null)
            return;

        RigidBody.linearVelocity = velocity;

        if (CreatureState != ECreatureState.Move)
            return;

        if (velocity.x < 0)
            LookRight = false;
        else if (velocity.x > 0)
            LookRight = true;
    }
    #endregion
}
