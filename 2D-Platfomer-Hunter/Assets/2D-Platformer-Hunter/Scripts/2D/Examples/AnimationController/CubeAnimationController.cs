using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DYP;

public class CubeAnimationController : MonoBehaviour
{
    private BasicMovementController2D m_Controller;

    [SerializeField]
    private Transform m_Appearance;
    public Transform Appearance { get { return m_Appearance; } }
    
    enum AnimState
    {
        Normal,
        IsStretching,
        IsRecovering
    }

    private AnimState m_AnimState = AnimState.Normal;

    private Tweener m_AnimTweener = null;

    private int m_Facing = 1;

    private void Awake()
    {
        m_Controller = GetComponent<BasicMovementController2D>();
    }

    private void Start()
    {
        m_Controller.OnFacingFlip += onFacingFlip;
        m_Controller.OnNormalJump += onJump;
        m_Controller.OnAirJump += onJump;
        m_Controller.OnWallJump += (Vector2 dir) => onJump();

        m_Controller.OnMotorStateChanged += (MotorState prev, MotorState curr) => 
        {
            if (prev != MotorState.Dashing && curr == MotorState.OnGround) onLanded();
        };
        m_Controller.OnWallSliding += onWallSliding;

        m_Controller.OnDash += onDash;
    }

    private void Update()
    {
        TweenManager.Instance.Update(Time.deltaTime);
    }

    private void onFacingFlip(int facing)
    {
        var originalScale = Appearance.localScale;
        originalScale.x *= facing * Mathf.Sign(originalScale.x);
        Appearance.localScale = new Vector3(facing, 1, 1);

        m_Facing = facing;
    }

    private void onJump()
    {
        m_AnimState = AnimState.IsStretching;

        if (m_AnimTweener != null)
        {
            m_AnimTweener.Abort();
            m_AnimTweener = null;
        }

        // map from startYScale to 2.0f
        float startYScale = 1.5f;
        float targetYScale = 1.0f;

        m_AnimTweener = TweenManager.Instance.Tween((float progress) =>
        {
            float yScale = Mathf.LerpUnclamped(startYScale, targetYScale, progress);
            float xScale = 1 / yScale;
            Appearance.transform.localScale = new Vector3(m_Facing * xScale, yScale, 1.0f);
        }).SetEase(EasingFunction.Ease.EaseOutCubic).SetTime(0.33f).SetEndCallback(() => m_AnimTweener = null);
    }

    private void onLanded()
    {
        m_AnimState = AnimState.Normal;

        if (m_AnimTweener != null)
        {
            m_AnimTweener.Abort();
            m_AnimTweener = null;
        }

        // map from startYScale to 1.0f
        float fallingSpeed = m_Controller.InputVelocity.y > 0? 0 : -m_Controller.InputVelocity.y;
        float scalar = 0.1f;

        float startXScale = fallingSpeed > (1 / scalar)? 1.5f : 1.0f + 0.5f * fallingSpeed * scalar;
        float targetXScale = 1.0f;

        m_AnimTweener = TweenManager.Instance.Tween((float progress) =>
        {
            float xScale = Mathf.LerpUnclamped(startXScale, targetXScale, progress);
            float yScale = 1 / xScale;
            Appearance.transform.localScale = new Vector3(m_Facing * xScale, yScale, 1.0f);
        }).SetEase(EasingFunction.Ease.EaseOutCubic).SetTime(0.33f).SetEndCallback(() => m_AnimTweener = null);
    }

    private void onWallSliding(int dir)
    {
        m_AnimState = AnimState.Normal;
    }

    private void onDash(int dir)
    {
        if (m_AnimTweener != null)
        {
            m_AnimTweener.Abort();
            m_AnimTweener = null;
        }

        // map from startYScale to 1.0f
        float startXScale = 2.0f;
        float targetXScale = 1.0f;

        m_AnimTweener = TweenManager.Instance.Tween((float progress) =>
        {
            float xScale = Mathf.LerpUnclamped(startXScale, targetXScale, progress);
            float yScale = 1 / xScale;
            Appearance.transform.localScale = new Vector3(m_Facing * xScale, yScale, 1.0f);
        }).SetEase(EasingFunction.Ease.EaseOutCubic).SetTime(0.33f).SetEndCallback(() => m_AnimTweener = null);
    }
}
