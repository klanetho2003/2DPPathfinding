using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

public class InputManager
{
    // KeyBoard
    private InputAction _moveAction;
    private Dictionary<KeyDownEvent, InputAction> _keyBindings = new();
    private Dictionary<KeyDownEvent, float> _lastPressedTime = new();
    private Dictionary<KeyDownEvent, bool> _isHolding = new();

    // 콜백
    public event Action<KeyDownEvent, KeyInputType> OnKeyInputHandler;

    // 설정값 -> Game에 따라 수정 필요
    private const float _holdThreshold = 0.5f;
    private const float _doubleTapThreshold = 0.25f;

    public void Init()
    {   
        InitMove();         // 이동 관련 Binding (모바일 원형 KeyPad 추가 고려)
        InitKeyBindings();  // etc Binding
    }

    public void OnUpdate()
    {
        HandleMoveInput();
        CheckHoldInputs();
    }

    public void Dispose()
    {
        _moveAction?.Disable();
        foreach (var action in _keyBindings.Values)
            action.Disable();
    }

    #region Init
    private void InitMove()
    {
        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("1DAxis")
            // .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            // .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");
        _moveAction.Enable();
    }

    private void InitKeyBindings()
    {
        AddKey(KeyDownEvent.Space, Key.Space);
        // AddKey(KeyDownEvent.Num1, Key.Digit1);
        // AddKey(KeyDownEvent.R, Key.Digit2);
    }

    private void AddKey(KeyDownEvent evt, Key key)
    {
        string path = $"<Keyboard>/{key.ToString().ToLower()}";
        var action = new InputAction(evt.ToString(), InputActionType.Button, path);

        action.started += ctx =>
        {
            float now = Time.time;

            // DoubleTap Check
            if (_lastPressedTime.TryGetValue(evt, out float lastTime))
            {
                if (now - lastTime <= _doubleTapThreshold)
                {
                    Debug.Log($"{evt} , {KeyInputType.DoubleTap}");
                    OnKeyInputHandler?.Invoke(evt, KeyInputType.DoubleTap);
                }   
            }

            _lastPressedTime[evt] = now;
            _isHolding[evt] = true; // Hold Trigger

            Debug.Log($"{evt} , {KeyInputType.Down}");
            OnKeyInputHandler?.Invoke(evt, KeyInputType.Down);
        };

        action.canceled += ctx =>
        {
            _isHolding[evt] = false;
            Debug.Log($"{evt} , {KeyInputType.Up}");
            OnKeyInputHandler?.Invoke(evt, KeyInputType.Up);
        };

        action.Enable();
        _keyBindings[evt] = action;
    }
    #endregion

    #region Hold Check
    private void CheckHoldInputs()
    {
        float now = Time.time;

        var holdingKeys = new List<KeyDownEvent>(_isHolding.Keys);

        foreach (var evt in holdingKeys)
        {
            if (_isHolding[evt] == false) continue;
            if (!_lastPressedTime.TryGetValue(evt, out float pressedTime)) continue;

            if (now - pressedTime >= _holdThreshold)
            {
                _isHolding[evt] = false;
                Debug.Log($"{evt} , {KeyInputType.Hold}");
                OnKeyInputHandler?.Invoke(evt, KeyInputType.Hold);
            }
        }
    }
    #endregion

    #region Move Handling
    private bool _isMoveKeyPressed = false;
    private static readonly Key[] _moveKeys = new Key[]
    {
        Key.LeftArrow, Key.RightArrow
    };

    private void HandleMoveInput()
    {
        #region Dirty Flag Check
        bool isAnyPressed = false;
        foreach (var key in _moveKeys)
        {
            if (Keyboard.current[key].isPressed)
            {
                isAnyPressed = true;
                break;
            }
        }

        // dirty flag 갱신
        _isMoveKeyPressed = isAnyPressed;
        #endregion

        if (!_isMoveKeyPressed)
        {
            Managers.Game.MoveDir = Vector2.zero;
            return;
        }

        float dir = _moveAction.ReadValue<float>();
        switch (dir)
        {
            case > 0:
                Managers.Game.MoveDir = Vector2.right;
                break;
            case < 0:
                Managers.Game.MoveDir = Vector2.left;
                break;
            default: // 0
                break;
        }
    }
    #endregion
}
