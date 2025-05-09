using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class BaseController : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CapsuleCollider2D Collider { get; protected set; }
    public Rigidbody2D RigidBody { get; protected set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public SortingGroup SortingGroup { get; private set; }

    public float ColliderRadius { get { return (Collider != null) ? Collider.size.y / 2 : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

    public int DataTemplateID { get; set; }

    #region Init & Disable
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Anim = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SortingGroup = gameObject.GetOrAddComponent<SortingGroup>();

        return true;
    }

    protected virtual void OnDisable()
    {
        Clear();
    }
    #endregion

    #region Update & FixedUpdate
    protected virtual void UpdateController() { }
    void Update()
    {
        UpdateController();
    }

    protected virtual void FixedUpdateController() { }
    void FixedUpdate()
    {
        FixedUpdateController();
    }
    #endregion

    #region Animation
    protected virtual void UpdateAnimation() { }

    protected virtual void SetAnimation(string dataLabel, string sortingLayerName, int sortingOrder)
    {
        if (Anim == null)
            return;

        // Animatior
        if (string.IsNullOrEmpty(dataLabel) == false)
            Anim.runtimeAnimatorController = Managers.Resource.Load<RuntimeAnimatorController>(dataLabel);

        SortingGroup.sortingLayerName = sortingLayerName;
        SortingGroup.sortingOrder = sortingOrder;
    }

    #region Look Helpers
    bool _lookRight = true;
    public bool LookRight
    {
        get { return _lookRight; }
        set
        {
            _lookRight = value;
            FlipX(!value);
        }
    }

    public virtual void FlipX(bool flag)
    {
        if (Anim == null)
            return;

        // On Sprite Flip
        SpriteRenderer.flipX = flag;
    }

    public void LookAtTarget(BaseController target)
    {
        Vector3 targetPos = target.transform.position;
        LookAtTarget(targetPos);
    }

    public void LookAtTarget(Vector3 targetPos)
    {
        Vector2 dir = targetPos - transform.position;
        LookAtTarget(dir);
    }

    public void LookAtTarget(Vector2 dir)
    {
        if (dir.x < 0)
            LookRight = true;
        else
            LookRight = false;
    }

    public static Vector3 GetLookAtRotation(Vector3 dir)
    {
        float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
        return new Vector3(0, 0, angle);
    }
    #endregion

    #endregion

    protected virtual void Clear()
    {
        if (Managers.Game == null)
            return;
    }
}
