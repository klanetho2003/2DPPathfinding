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

    // 설정값
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
    }

    public void Dispose()
    {
        _moveAction?.Disable();
        foreach (var action in _keyBindings.Values)
            action.Disable();
    }

    #region === Initialization ===

    private void InitMove()
    {
        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
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

            // DoubleTap 검사
            if (_lastPressedTime.TryGetValue(evt, out float lastTime))
            {
                if (now - lastTime <= _doubleTapThreshold)
                    OnKeyInputHandler?.Invoke(evt, KeyInputType.DoubleTap);
            }

            _lastPressedTime[evt] = now;
            _isHolding[evt] = true;

            OnKeyInputHandler?.Invoke(evt, KeyInputType.Down);
        };

        action.canceled += ctx =>
        {
            _isHolding[evt] = false;
            OnKeyInputHandler?.Invoke(evt, KeyInputType.Up);
        };

        action.Enable();
        _keyBindings[evt] = action;
    }

    #endregion

    #region === Hold 체크 ===

    private void CheckHoldInputs()
    {
        float now = Time.time;
        foreach (var pair in _isHolding)
        {
            if (pair.Value == false) continue;

            var evt = pair.Key;
            float pressedTime = _lastPressedTime.ContainsKey(evt) ? _lastPressedTime[evt] : -1f;
            if (pressedTime < 0f) continue;

            if (now - pressedTime >= _holdThreshold)
            {
                _isHolding[evt] = false; // 1회만 호출
                OnKeyInputHandler?.Invoke(evt, KeyInputType.Hold);
            }
        }
    }

    #endregion

    #region Move Handling
    private void HandleMoveInput()
    {
        float dir = _moveAction.ReadValue<float>();
        switch (dir)
        {
            case > 0:
                Debug.Log("->");
                break;
            case < 0:
                Debug.Log("<-");
                break;
            default: // 0
                Debug.Log("None Dir");
                break;
        }
    }
    #endregion
}
