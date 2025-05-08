using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class BaseController : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; protected set; }
    public Rigidbody2D RigidBody { get; protected set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    public float ColliderRadius { get { return (Collider != null) ? Collider.radius : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

    public int DataTemplateID { get; set; }

    #region Init & Disable
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Anim = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        return true;
    }

    protected virtual void OnDisable()
    {
        Clear();
    }
    #endregion

    #region Update & FixedUpdate
    public virtual void UpdateController() { }
    void Update()
    {
        UpdateController();
    }

    public virtual void FixedUpdateController() { }
    void FixedUpdate()
    {
        FixedUpdateController();
    }
    #endregion

    #region Animation
    protected virtual void UpdateAnimation() { }


    #endregion

    protected virtual void Clear()
    {
        if (Managers.Game == null)
            return;
    }
}
