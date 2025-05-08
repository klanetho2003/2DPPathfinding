using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public abstract class UI_Base : InitBase
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);       // 매핑 (게임 오브젝트 전용)
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);    // 매핑

            if (objects[i] == null)
                Debug.Log($"Failed to Bind -> {names[i]}");
        }
    }

    public void BindObjects(Type type) { Bind<GameObject>(type); }
    public void BindImages(Type type) { Bind<Image>(type); }
    public void BindTexts(Type type) { Bind<TMP_Text>(type); }
    public void BindButtons(Type type) { Bind<Button>(type); }
    public void BindToggles(Type type) { Bind<Toggle>(type); }
    public void BindSliders(Type type) { Bind<Slider>(type); }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }
    protected Slider GetSliders(int idx) { return Get<Slider>(idx); }

    public static void BindEvent(GameObject go, params (UIEvent eventType, Action<PointerEventData> callback)[] handlers)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        foreach (var (type, callback) in handlers)
        {
            if (type == UIEvent.None || callback == null)
                continue;

            // 동일 CallBack 제거
            Unsubscribe(evt, type, callback);
            // 새로 추가
            Subscribe(evt, type, callback);
        }
    }

    #region Helpers
    private static void Unsubscribe(UI_EventHandler evt, UIEvent type, Action<PointerEventData> cb)
    {
        switch (type)
        {
            case UIEvent.PointerEnter: evt.OnPointerEnterHandler -= cb; break;
            case UIEvent.PointerExit: evt.OnPointerExitHandler -= cb; break;
            case UIEvent.PointerDown: evt.OnPointerDownHandler -= cb; break;
            case UIEvent.PointerUp: evt.OnPointerUpHandler -= cb; break;
            case UIEvent.Click: evt.OnClickHandler -= cb; break;
            case UIEvent.BeginDrag: evt.OnDragHandler -= cb; break;
            case UIEvent.Drag: evt.OnDragHandler -= cb; break;
            case UIEvent.EndDrag: evt.OnDragHandler -= cb; break;
        }
    }

    // 내부 헬퍼: 서브스크라이브
    private static void Subscribe(UI_EventHandler evt, UIEvent type, Action<PointerEventData> cb)
    {
        switch (type)
        {
            case UIEvent.PointerEnter: evt.OnPointerEnterHandler += cb; break;
            case UIEvent.PointerExit: evt.OnPointerExitHandler += cb; break;
            case UIEvent.PointerDown: evt.OnPointerDownHandler += cb; break;
            case UIEvent.PointerUp: evt.OnPointerUpHandler += cb; break;
            case UIEvent.Click: evt.OnClickHandler += cb; break;
            case UIEvent.BeginDrag: evt.OnDragHandler += cb; break;
            case UIEvent.Drag: evt.OnDragHandler += cb; break;
            case UIEvent.EndDrag: evt.OnDragHandler += cb; break;
        }
    }
    #endregion
}
