using Data;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Player : Creature
{
    public PlayerData PlayerData { get { return (PlayerData)CreatureData; } }

    [SerializeField, Tooltip("대쉬 속도")]
    private float dashSpeed = 20f;
    [SerializeField, Tooltip("대쉬 지속 시간")]
    private float dashDuration = 0.2f;

    private bool _isDashing;
    private float _dashTimeLeft;



    [SerializeField]
    private bool _isJumpKeyDown = false;

    private float _lastGroundedTime; // 마지막으로 지면에 닿은 시점

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
                                    return;
                            }
                            
                            jumpDir = Vector2.up;
                        }

                        DoJump(jumpDir.normalized, force);
                        _isJumpKeyDown = true;
                    }
                    #endregion

                    if (key == EKeyDownEvent.D && CanDash())
                    {
                        StartDash();
                    }
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
                    if (RigidBody.linearVelocityY > PlayerData.JumpToMidSpeedThreshold)
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
    #endregion

    protected override void UpdateController()
    {
        base.UpdateController();

        UpdateCoyoteTimer(); // lastGroundedTime 갱신
    }

    private void UpdateCoyoteTimer()
    {
        // 지면에 닿을 때마다 시간을 리셋
        if (IsGrounded == false)
            return;
        
        _lastGroundedTime = Time.time;
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

    protected override void UpdateIdle()
    {
        if (MoveDir != Vector2.zero) { CreatureState = ECreatureState.Move; return; }
    }

    protected override void UpdateMove()
    {
        if (MoveDir == Vector2.zero) { CreatureState = ECreatureState.Idle; return; }
    }

    protected override void UpdateJump()
    {
        // 하강
        if (RigidBody.linearVelocityY < PlayerData.MidToFallSpeedThreshold)
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

    protected override void UpdateDash()
    {
        _dashTimeLeft -= Time.deltaTime;
        if (_dashTimeLeft <= 0f)
        {
            EndDash();
        }
        else
        {
            // 계속 대쉬 속도 유지 (To Do : 중력 무시 or 약간만 적용)
            Vector2 v = RigidBody.linearVelocity;
            RigidBody.linearVelocity = new Vector2(Mathf.Sign(v.x) * dashSpeed, 0f);
        }
    }
    #endregion

    #region Jump Method
    protected override void DoJump(Vector2 dir, float force)
    {
        base.DoJump(dir, force);
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

    #region Dash Method
    private void StartDash()
    {
        _isDashing = true;
        _dashTimeLeft = dashDuration;
        CreatureState = ECreatureState.Dash;

        Vector2 dir = RawMoveInput.x != 0 ? RawMoveInput.normalized : (LookRight ? Vector2.right : Vector2.left);
        SetRigidBodyVelocity(dir.x * dashSpeed);
    }

    private void EndDash()
    {
        _isDashing = false;
        CreatureState = ECreatureState.Move;
        // 대쉬 후엔 일반 물리 & gravity 적용
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

    private bool IsGroundedWithCoyote()
    {
        return IsGrounded || (Time.time - _lastGroundedTime <= PlayerData.CoyoteTimeDuration);
    }

    // Dash
    private bool CanDash()
    {
        // To Do : 지면 혹은 공중 대쉬 허용 여부, 쿨다운 체크 등

        return !_isDashing;
    }
    #endregion
}
