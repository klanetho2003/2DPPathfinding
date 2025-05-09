using UnityEngine;
using static Define;

public class Player : Creature
{
    #region Init & SetInfo
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChange;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChange;

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        Debug.Log($"CenterPosition -> {CenterPosition}");
    }
    #endregion

    #region Event Handling
    void HandleOnMoveDirChange(Vector2 dir)
    {
        MoveDir = dir;
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
                Anim.Play("Move");
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
}
