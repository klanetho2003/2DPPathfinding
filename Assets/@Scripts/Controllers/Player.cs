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

    void HandleOnKeyInputHandler(KeyDownEvent key, KeyInputType inputType)
    {
        switch (inputType)
        {
            case KeyInputType.Down:
                {
                    #region Space
                    if (key == KeyDownEvent.Space)
                    {
                        _isJumpKeyDown = true;
                        DoJump(Vector2.up, false);
                    }
                    #endregion
                }
                break;
            case KeyInputType.Up:
                {
                    #region Space
                    if (key == KeyDownEvent.Space)
                    {
                        _isJumpKeyDown = false;
                    }
                    #endregion
                }
                break;
            case KeyInputType.Hold:
                break;
            case KeyInputType.DoubleTap:
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
        else if (IsGrounded && RigidBody.linearVelocityY == 0)
            CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateFall()
    {
        if (IsGrounded)
            CreatureState = ECreatureState.Idle;
    }
    #endregion

    protected override void DoJump(Vector2 dir, bool onWall)
    {
        base.DoJump(dir, onWall);
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
}
