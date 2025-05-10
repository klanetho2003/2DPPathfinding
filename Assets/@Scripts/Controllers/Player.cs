using UnityEngine;
using static Define;

public class Player : Creature
{
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
                    if (key == KeyDownEvent.Space)
                        DoJump(Vector2.up, false);
                }
                break;
            case KeyInputType.Up:
                break;
            case KeyInputType.Hold:
                break;
            case KeyInputType.DoubleTap:
                break;
        }
    }
    #endregion

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
                Anim.Play("Jump");
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
    #endregion

    private void DoJump(Vector2 dir, bool onWall)
    {
        RigidBody.linearVelocity = new Vector2(RigidBody.linearVelocity.x, 0);
        RigidBody.linearVelocity += dir * JumpForce;
    }
}
