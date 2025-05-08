using System;
using UnityEngine;
using static Define;

public class InputManager
{
    public event Action<KeyDownEvent> OnKeyDownHandler = null;

    public void OnUpdate()
    {
        if (Input.anyKey == false)
        {
            // Managers.Game.MoveDir = Vector2.zero;
        }
        else
        {
            OnKeyInput();
        }
    }

    void OnKeyInput()
    {
        if (OnKeyDownHandler == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnKeyDownHandler.Invoke(KeyDownEvent.Space);
        }
    }
}
