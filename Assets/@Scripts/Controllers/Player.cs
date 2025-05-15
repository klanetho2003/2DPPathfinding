using System.Collections;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Player : Creature
{
    public PlayerData PlayerData { get { return (PlayerData)CreatureData; } }
    public PlayerMovementData PlayerMovementData { get { return (PlayerMovementData)CreatureMovementData; } }

    private bool _isJumpKeyDown = false;

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

        Managers.Input.OnKeyInputHandler -= HandleOnKeyInputHandler;
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
                    if (key == EKeyDownEvent.Space)
                    {
                        Vector2 jumpDir = Vector2.zero;
                        float force = JumpForce;
                        
                        if (IsGrounded == false && CanLeftWallJump())
                        {
                            jumpDir = new Vector2(1f, 2f);
                            force = force  * 1.5f;
                        }   
                        else if (IsGrounded == false && CanRightWallJump())
                        {
                            jumpDir = new Vector2(-1f, 2f);
                            force = force * 1.5f;
                        }   
                        else if (IsGroundedWithCoyote())
                        {
                            switch (CreatureState)
                            {
                                case ECreatureState.Jump:
                                case ECreatureState.Fall:
                                case ECreatureState.Wall:
                                case ECreatureState.Dash:
                                    return;
                            }
                            
                            jumpDir = Vector2.up;
                        }

                        DoJump(jumpDir.normalized, force);
                        _isJumpKeyDown = true;
                    }
                    #endregion

                    #region D - Dash
                    if (key == EKeyDownEvent.D && CanDash())
                    {
                        StartDash(RawMoveInput);
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

        // Better Jump: 강화 낙하 & 가변 점프 높이
        ApplyBetterJump();

        if (CreatureState == ECreatureState.Dash)
            RigidBody.linearVelocity = _dashDirection * PlayerMovementData.DashSpeed;
    }

    protected override void UpdateGrounded()
    {
        base.UpdateGrounded();

        Vector3Int CellPos = Managers.Map.World2Cell(transform.position);

        Managers.Map.MoveTo(this, CellPos);
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
            case ECreatureState.Jump:
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
                break;
        }
    }
    #endregion

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    #region State Pattern
    protected override void OnStateChange(ECreatureState brefore, ECreatureState after)
    {
        switch (after)
        {
            case ECreatureState.Move:
                break;
            case ECreatureState.Jump:
                break;
            case ECreatureState.Fall:
                break;
            case ECreatureState.Dash:
                break;

            case ECreatureState.Idle:
            case ECreatureState.Wall:
                RigidBody.linearVelocityY = 0;
                break;
        }
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();

        // 하강
        if (RigidBody.linearVelocityY < PlayerMovementData.MidToFallSpeedThreshold)
            CreatureState = ECreatureState.Fall;
    }

    protected override void UpdateJump()
    {
        // 하강
        if (RigidBody.linearVelocityY < PlayerMovementData.MidToFallSpeedThreshold)
            CreatureState = ECreatureState.Fall;

        else if (OnLeftWall && IsRightKeyInput()) return;   // Wall Jump를 더 후하게
        else if (OnRightWall && IsLeftKeyInput()) return;   // Wall Jump를 더 후하게
        else if (OnWall && RawMoveInput != Vector2.zero)         // Wall Grap
            CreatureState = ECreatureState.Wall;

        // 착지
        else if (IsGrounded && Util.IsEqualValue(RigidBody.linearVelocityY, 0))
            CreatureState = ECreatureState.Idle;

        // Jump Mid 구간으로 Animation을 바꾸기 위함
        if (CreatureState == ECreatureState.Jump)
            UpdateAnimation();
    }

    protected override void UpdateFall()
    {
        if (OnLeftWall && IsRightKeyInput())        return; // Fall을 더 후하게
        else if (OnRightWall && IsLeftKeyInput())   return; // Fall을 더 후하게
        else if (OnWall && RawMoveInput != Vector2.zero)
            CreatureState = ECreatureState.Wall;

        else if (IsGrounded)
            CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateWall()
    {
        if (OnWall == false)
        {
            CreatureState = ECreatureState.Fall;
            return;
        }

        ApplyWallSlide();

        // 지면_X
        if (IsGrounded == false)
        {
            switch (RawMoveInput.x)
            {
                // Move Input '->'
                case 1:     if (OnLeftWall == true)     CreatureState = ECreatureState.Fall;
                    return;

                // Move Input '<-'
                case -1:    if (OnRightWall == true)    CreatureState = ECreatureState.Fall;
                    return;
            }
        }

        // 지면_O
        else
        {
            switch (RawMoveInput.x)
            {
                // Move Input 'None'
                case 0:     CreatureState = ECreatureState.Idle;
                    return;

                // Move Input '->'
                case 1:     if (OnLeftWall == true)     CreatureState = ECreatureState.Idle;
                    return;

                // Move Input '<-'
                case -1:    if (OnRightWall == true)    CreatureState = ECreatureState.Idle;
                    return;
            }
        }

        // 최최최종 Plan
        if (OnWall == false && CreatureState == ECreatureState.Wall)
            CreatureState = ECreatureState.Fall;
    }

    protected override void UpdateDash()
    {
        _dashTimeLeft -= Time.deltaTime;

        // 벽
        if (OnWall)
            EndDash(ECreatureState.Wall);
        // Dash Time Out
        else if (_dashTimeLeft <= 0f)
            EndDash(ECreatureState.Move);
    }
    #endregion

    #region Jump Method
    protected override void DoJump(Vector2 dir, float force)
    {
        base.DoJump(dir, force);
    }

    private void ApplyBetterJump()
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

    #region Wall
    private void ApplyWallSlide()
    {
        // 아래로 떨어지는 중일 때만
        if (RigidBody.linearVelocityY < 0f)
        {
            // 더 빠르게 내려가지 않도록 Clamp
            float maxDown = -MovementValues.wallSlideMaxSpeed;
            if (RigidBody.linearVelocityY < maxDown)
            {
                RigidBody.linearVelocityY = maxDown;
            }
        }
    }
    #endregion

    #region Dash Method

    private float _dashTimeLeft;
    private Vector2 _dashDirection = Vector2.zero;
    public float RemainDashCoolTime { get; set; }

    private void StartDash(Vector2 dir)
    {
        if (dir == Vector2.zero) return;

        _dashTimeLeft = PlayerMovementData.DashDuration;
        _dashDirection = dir.normalized;

        RigidBody.linearVelocity = Vector2.zero;
        RigidBody.linearVelocity = dir * PlayerMovementData.DashSpeed;

        CreatureState = ECreatureState.Dash;
        StartCoroutine(CoCountdownDashCool());
    }

    private void EndDash(ECreatureState afterState)
    {
        CreatureState = afterState;
    }

    private IEnumerator CoCountdownDashCool()
    {
        RemainDashCoolTime = PlayerMovementData.DashCoolTime;
        yield return new WaitForSeconds(RemainDashCoolTime);
        RemainDashCoolTime = 0;
    }

    #endregion

    #region Helper
    // Jump
    private bool CanLeftWallJump()
    {
        return OnLeftWall && IsLeftKeyInput() == false;
    }
    private bool CanRightWallJump()
    {
        return OnRightWall && IsRightKeyInput() == false;
    }

    private bool IsLeftKeyInput()
    {
        return MoveDir.x < 0;
    }
    private bool IsRightKeyInput()
    {
        return MoveDir.x > 0;
    }

    // Dash
    private bool CanDash()
    {
        bool canDash = (RemainDashCoolTime == 0) && (CreatureState != ECreatureState.Dash);

        return canDash;
    }
    #endregion
}
