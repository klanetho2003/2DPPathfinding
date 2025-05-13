using UnityEngine;
using static Define;

public class Enemy : Creature
{
    public virtual Player Target { get; private set; }

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
        if (Target.IsValid() == false)
            return;

        base.FixedUpdateController();

        MoveDir = (Target.transform.position - transform.position).normalized;
    }
}
