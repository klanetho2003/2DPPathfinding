using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    #region Manager
    public enum EScene
    {
        Unknown,
        TitleScene,
        GameScene,
    }

    public enum EKeyDownEvent
    {
        None = -1,

        D = 1,
        Space = 100,
    }

    public enum EKeyInputType
    {
        None = -1,

        Down,
        Up,
        Hold,
        DoubleTap
    }

    public enum Sound
    {
        Bgm,
        Effect,
    }

    public enum UIEvent
    {
        None,
        PointerEnter,
        PointerExit,
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        BeginDrag,
        Drag,
        EndDrag,
    }
    #endregion

    public enum EObjectType
    {
        None,
        Player,
        Enemy,
    }

    public enum ECreatureState
    {
        None,
        Idle,
        Move,
        Jump,
        Fall,
        Wall,
        Dash,
    }

    public enum ETileType
    {
        None = -1,
        HorizontalOnly = 0,
        Jumpable = 1,
        DeadEnd = 2
    }

    public enum EdgeType
    {
        Horizontal,
        Jump,
    }

    public enum ELayer
    {
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Dummy1 = 3,
        Water = 4,
        UI = 5,
        Creature = 6,
        Ground = 10
    }

    public enum EFindPathResult
    {
        Fail_LerpCell,
        Fail_NoPath,
        Fail_MoveTo,
        Success,
    }

    // Hard Coding
    public static class SortingLayers
    {
        public const int CREATURE = 100;
    }

    public const int MONSTER_DEFAULT_MOVE_DEPTH = 10;
}
