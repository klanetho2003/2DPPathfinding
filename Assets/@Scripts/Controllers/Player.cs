using UnityEngine;
using static Define;

public class Player : Creature
{
    [SerializeField]
    bool _isJumpKeyDown = false;

    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Player;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChange;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChange;

        MovementValues = Managers.Resource.Load<MoveMentValues>($"MoveMentValues_{ObjectType}");

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        Managers.Input.OnKeyInputHandler += HandleOnKeyInputHandler;
    }
    #endregion

    #region Event Handling
    void HandleOnMoveDirChange(Vector2 dir)
    {
        MoveDir = dir;
    }

    void HandleOnKeyInputHandler(EKeyDownEvent key, EKeyInputType inputType)
    {
        // Input Habdle
        switch (inputType)
        {
            case EKeyInputType.Down:
                {
                    #region Space
                    if (key == EKeyDownEvent.Space && IsGrounded)
                    {
                        _isJumpKeyDown = true;
                        DoJump(Vector2.up);
                    }
                    else if (true)
                    {
                        _isJumpKeyDown = true;
                        Vector2 jumpDir = OnLeftWall ? Vector2.right : Vector2.left;
                        DoJump((Vector2.up / 1.5f) + (jumpDir / 1.5f));
                    }
                    #endregion
                }
                break;
            case EKeyInputType.Up:
                {
                    #region Space
                    if (key == EKeyDownEvent.Space)
                    {
                        _isJumpKeyDown = false;
                    }
                    #endregion
                }
                break;
            case EKeyInputType.Hold:
                break;
            case EKeyInputType.DoubleTap:
                break;
        }
    }
    #endregion

    protected override void FixedUpdateController()
    {
        base.FixedUpdateController();
    }

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
                    if (RigidBody.linearVelocityY > 1.5f)
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
        }
    }

    #region State Pattern
    protected override void UpdateIdle()
    {
        if (_moveDir != Vector2.zero) { CreatureState = ECreatureState.Move; return; }
    }

    protected override void UpdateMove()
    {
        if (_moveDir == Vector2.zero) { CreatureState = ECreatureState.Idle; return; }
    }

    protected override void UpdateJump()
    {
        UpdateAnimation(); // Jump Mid 구간으로 Animation을 바꾸기 위해 호출

        if (RigidBody.linearVelocityY < -1.5f)
            CreatureState = ECreatureState.Fall;

        else if (OnWall)
            CreatureState = ECreatureState.Wall;

        else if (IsGrounded && Util.IsEqualValue(RigidBody.linearVelocityY, 0))
            CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateFall()
    {
        if (OnWall)
            CreatureState = ECreatureState.Wall;

        else if (IsGrounded)
            CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateWall()
    {
        // 지면_X
        if (IsGrounded == false)
        {
            switch (MoveDir.x)
            {
                case 1: // Move Input '->'
                    {
                        if (OnLeftWall == true)
                            CreatureState = ECreatureState.Fall;
                    }
                    break;
                case -1: // Move Input '<-'
                    {
                        if (OnRightWall == true)
                            CreatureState = ECreatureState.Fall;
                    }
                    break;
            }
        }

        // 지면_O
        else
        {
            switch (MoveDir.x)
            {
                case 0: // Move Input 'None'
                    CreatureState = ECreatureState.Idle;
                    break;
                case 1: // Move Input '->'
                    {
                        if (OnLeftWall == true)
                            CreatureState = ECreatureState.Idle;
                    }
                    break;
                case -1: // Move Input '<-'
                    {
                        if (OnRightWall == true)
                            CreatureState = ECreatureState.Idle;
                    }
                    break;
            }
        }

        // 최최최종 Plan
        if (OnWall == false && CreatureState == ECreatureState.Wall)
            CreatureState = ECreatureState.Fall;
    }
    #endregion

    #region Jump Method
    protected override void DoJump(Vector2 dir)
    {
        base.DoJump(dir);
    }

    protected override void ApplyBetterJump()
    {
        if (IsGrounded)
            return;

        float vy = RigidBody.linearVelocityY;
        float multiplier = 1f;

        if (vy < 0f)
        {
            // 빠른 낙하
            multiplier = MovementValues.fallMultiplier;
        }
        else if (vy > 0f && !_isJumpKeyDown)
        {
            // 짧은 점프
            multiplier = MovementValues.lowJumpMultiplier;
        }

        // 적용
        RigidBody.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1f) * Time.fixedDeltaTime;
    }
    #endregion
}
