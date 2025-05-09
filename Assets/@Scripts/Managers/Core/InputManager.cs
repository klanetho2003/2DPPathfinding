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
            Debug.Log("None Dir");
        }
        else
        {
            OnDirInput();
            OnKeyInput();
        }
    }

    const string DIR = "Horizontal";
    void OnDirInput()
    {
        float horizontalDir = Input.GetAxisRaw(DIR);

        switch (horizontalDir)
        {
            case 1:     // Input ->
                Debug.Log("->");
                break;
            case -1:    // Input <-
                Debug.Log("<-");
                break;
        }
    }

    void OnKeyInput()
    {
        if (OnKeyDownHandler == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnKeyDownHandler.Invoke(KeyDownEvent.Space);
            Debug.Log("Space");
        }
    }
}
