using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

public class InputManager
{
    // KeyBoard
    private InputAction _moveAction;
    private Dictionary<EKeyDownEvent, InputAction> _keyBindings = new();
    private Dictionary<EKeyDownEvent, float> _lastPressedTime = new();
    private Dictionary<EKeyDownEvent, bool> _isHolding = new();

    // 콜백
    public event Action<EKeyDownEvent, EKeyInputType> OnKeyInputHandler;

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
        // _moveAction = new InputAction("Move", InputActionType.Value); // 좌우만
        _moveAction = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Vector2");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",     "<Keyboard>/upArrow")
            .With("Down",   "<Keyboard>/downArrow")
            .With("Left",   "<Keyboard>/leftArrow")
            .With("Right",  "<Keyboard>/rightArrow");

        _moveAction.Enable();
    }

    private void InitKeyBindings()
    {
        AddKey(EKeyDownEvent.Space, Key.Space);
        AddKey(EKeyDownEvent.D, Key.D);
        // AddKey(KeyDownEvent.R, Key.Digit2);
    }

    private void AddKey(EKeyDownEvent evt, Key key)
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
                    Debug.Log($"{evt} , {EKeyInputType.DoubleTap}");
                    OnKeyInputHandler?.Invoke(evt, EKeyInputType.DoubleTap);
                }   
            }

            _lastPressedTime[evt] = now;
            _isHolding[evt] = true; // Hold Trigger

            Debug.Log($"{evt} , {EKeyInputType.Down}");
            OnKeyInputHandler?.Invoke(evt, EKeyInputType.Down);
        };

        action.canceled += ctx =>
        {
            _isHolding[evt] = false;
            Debug.Log($"{evt} , {EKeyInputType.Up}");
            OnKeyInputHandler?.Invoke(evt, EKeyInputType.Up);
        };

        action.Enable();
        _keyBindings[evt] = action;
    }
    #endregion

    #region Hold Check
    private void CheckHoldInputs()
    {
        float now = Time.time;

        var holdingKeys = new List<EKeyDownEvent>(_isHolding.Keys);

        foreach (var evt in holdingKeys)
        {
            if (_isHolding[evt] == false) continue;
            if (!_lastPressedTime.TryGetValue(evt, out float pressedTime)) continue;

            if (now - pressedTime >= _holdThreshold)
            {
                _isHolding[evt] = false;
                Debug.Log($"{evt} , {EKeyInputType.Hold}");
                OnKeyInputHandler?.Invoke(evt, EKeyInputType.Hold);
            }
        }
    }
    #endregion

    #region Move Handling
    private bool _isMoveKeyPressed = false;
    private static readonly Key[] _moveKeys = new Key[]
    {
        Key.LeftArrow, Key.RightArrow, Key.UpArrow, Key.DownArrow
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

        // 1) Vector2로 입력 읽기
        Vector2 input = _moveAction.ReadValue<Vector2>();
        float vx =  input.x > 0 ?
                    1f  : input.x < 0 ?
                    -1f : 0f;
        float vy =  input.y > 0 ?
                    1f  : input.y < 0 ?
                    -1f : 0f;

        // 2) 최종 이동 방향 설정
        Managers.Game.MoveDir = new Vector2(vx, vy);
    }
    #endregion
}
