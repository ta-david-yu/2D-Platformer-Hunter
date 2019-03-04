using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -- BasicCharacterController2D
 * 
 *  Handles Gravity, Jump and Horizontal Movement
 */
[RequireComponent(typeof(CharacterMotor2D))]
public class BasicMovementController2D : MonoBehaviour
{
    [System.Serializable]
    class MovementSettings
    {
        public float Speed = 8;
        public float OnLadderSpeed = 5;
        public float AccelerationTimeAirborne = .1f;
        public float AccelerationTimeGrounded = .1f;
    }

    [System.Serializable]
    class JumpSettings
    {
        public bool HasVariableJumpHeight = true;
        public int AirJumpAllowed = 1;
        public float MaxHeight = 3.5f;
        public float MinHeight = 1.0f;
        public float TimeToApex = .4f;
        public float OnLadderJumpForce = 0.0f;
    }

    [System.Serializable]
    class WallJumpSettings
    {
        public bool CanWallJump = true;

        public float WallSlidingSpeedMax = 2;
        public float WallStickTime = 0.15f;

        [HideInInspector]
        public float WallStickTimer = 0.0f;

        public Vector2 ClimbForce = new Vector2(12, 16);
        public Vector2 OffForce = new Vector2(8, 15);
        public Vector2 LeapForce = new Vector2(18, 17);
    }

    [System.Serializable]
    class DashState
    {
        public BaseDashModule Module;

        private int m_DashDir;
        public int DashDir { get { return m_DashDir; } }      // 1: right, -1: left

        private float m_DashTimer;
        private float m_PrevDashTimer;

        public void Start(int dashDir, float timeStep)
        {
            m_DashDir = dashDir;
            m_PrevDashTimer = 0.0f;
            m_DashTimer = timeStep;
        }

        public void _Update(float timeStep)
        {
            m_PrevDashTimer = m_DashTimer;
            m_DashTimer += timeStep;

            if (m_DashTimer > Module.DashTime)
            {
                m_DashTimer = Module.DashTime;
            }
        }

        public float GetDashSpeed()
        {
            if (Module != null)
            {
                return Module.GetDashSpeed(m_PrevDashTimer, m_DashTimer);
            }
            else
            {
                return 0;
            }
        }

        public float GetDashProgress()
        {
            return Module.GetDashProgress(m_DashTimer);
        }
    }

    [System.Serializable]
    class ActionState
    {
        public BaseActionModule Module;

        private int m_ActionDir;
        public int ActionDir { get { return m_ActionDir; } }

        private float m_ActionTimer;
        private float m_PrevActionTimer;

        public void Start(int actionDir)
        {
            m_ActionDir = actionDir;
            m_PrevActionTimer = 0.0f;
            m_ActionTimer = 0.0f;
        }

        public void _Update(float timeStep)
        {
            m_PrevActionTimer = m_ActionTimer;
            m_ActionTimer += timeStep;

            if (m_ActionTimer > Module.ActionTime)
            {
                m_ActionTimer = Module.ActionTime;
            }
        }

        public Vector2 GetActionVelocity()
        {
            if (Module != null)
            {
                return Module.GetActionSpeed(m_PrevActionTimer, m_ActionTimer);
            }
            else
            {
                return Vector2.zero;
            }
        }
        
        public float GetActionProgress()
        {
            return Module.GetActionProgress(m_ActionTimer);
        }
    }

    [System.Serializable]
    class OnLadderState
    {
        public bool IsInLadderArea = false;
        public Bounds Area = new Bounds(Vector3.zero, Vector3.zero);
        public Bounds BottomArea = new Bounds(Vector3.zero, Vector3.zero);
        public Bounds TopArea = new Bounds(Vector3.zero, Vector3.zero);
        public LadderZone AreaZone = LadderZone.Bottom;

        public bool HasRestrictedArea = false;
        public Bounds RestrictedArea = new Bounds();
        public Vector2 RestrictedAreaTopRight = new Vector2(Mathf.Infinity, Mathf.Infinity);
        public Vector2 RestrictedAreaBottomLeft = new Vector2(-Mathf.Infinity, -Mathf.Infinity);
    }

    [Header("Reference")]
    private CharacterMotor2D m_Motor;
    public Collider2D MotorCollider
    {
        get
        {
            return m_Motor.Collider2D;
        }
    }

    private BaseInputDriver m_InputDriver;

    [Header("Settings")]
    [SerializeField]
    private MovementSettings m_MovementSettings;

    [SerializeField]
    private JumpSettings m_JumpSettings;

    [SerializeField]
    private WallJumpSettings m_WallJumpSettings;
    
    private MotorState m_MotorState;

    private float m_Gravity;
    private bool m_ApplyGravity = true;

    private float m_MaxJumpSpeed;
    private float m_MinJumpSpeed;

    private int m_AirJumpCounter = 0;

    private int m_FacingDirection = 1;
    public int FacingDirection
    {
        get { return m_FacingDirection; }
        private set
        {
            int oldFacing = m_FacingDirection;
            m_FacingDirection = value;

            if (m_FacingDirection != oldFacing)
            {
                OnFacingFlip(m_FacingDirection);
            }
        }
    }
    
    [SerializeField]
    private DashState m_DashState = new DashState();

    [SerializeField]
    private ActionState m_ActionState = new ActionState();

    public BaseDashModule DashModule { get { return m_DashState.Module; } private set { m_DashState.Module = value; } }

    public BaseActionModule ActionModule { get { return m_ActionState.Module; } private set { m_ActionState.Module = value; } }

    private OnLadderState m_OnLadderState = new OnLadderState();

    [Header("State")]
    private Vector3 m_Velocity;
    public Vector3 InputVelocity { get { return m_Velocity; } }

    private float m_VelocityXSmoothing;

    // Action
    public Action<MotorState, MotorState> OnMotorStateChanged = delegate { };

    public Action OnJump = delegate { };                // on all jump! // OnEnterStateJump
    public Action OnJumpEnd = delegate { };             // on jump -> falling  // OnLeaveStateJump

    public Action OnNormalJump = delegate { };          // on ground jump
    public Action OnLadderJump = delegate { };          // on ladder jump
    public Action OnAirJump = delegate { };             // on air jump
    public Action<Vector2> OnWallJump = delegate { };   // on wall jump

    public Action<int> OnDash = delegate { };           // int represent dash direction
    public Action<float> OnDashStay = delegate { };     // float represent action progress
    public Action OnDashEnd = delegate { };

    public Action<int> OnWallSliding = delegate { };    // int represnet wall direction: 1 -> right, -1 -> left
    public Action OnWallSlidingEnd = delegate { };

    public Action OnLanded = delegate { };              // on grounded

    public Action<MotorState> OnResetJumpCounter = delegate { };
    public Action<int> OnFacingFlip = delegate { };

    public Action<int> OnAction = delegate { };
    public Action<float> OnActionStay = delegate { };
    public Action OnActionEnd = delegate { };

    // Condition
    public Func<bool> CanAirJumpFunc = null;

    #region Monobehaviour

    private void Awake()
    {
        m_Motor = GetComponent<CharacterMotor2D>();
        m_InputDriver = GetComponent<BaseInputDriver>();

        if (m_InputDriver == null)
        {
            Debug.LogWarning("An InputDriver is needed for a BasicCharacterController2D");
        }
    }

    // Temporary Start
    private void Start()
    {
        Init();
    }

    // Temporary Update
    private void FixedUpdate()
    {
        _Update(Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        if (IsInLadderArea())
        {
            Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawCube(m_OnLadderState.Area.center, m_OnLadderState.Area.extents * 2);
            
            Gizmos.color = (m_OnLadderState.AreaZone == LadderZone.Top) ? new Color(1.0f, 0.92f, 0.016f, 1.0f) : Color.white;
            Gizmos.DrawWireCube(m_OnLadderState.TopArea.center, m_OnLadderState.TopArea.extents * 2);
            
            Gizmos.color = (m_OnLadderState.AreaZone == LadderZone.Bottom) ? new Color(1.0f, 0.92f, 0.016f, 1.0f) : Color.white;
            Gizmos.DrawWireCube(m_OnLadderState.BottomArea.center, m_OnLadderState.BottomArea.extents * 2);
            

            if (IsRestrictedOnLadder())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(m_OnLadderState.RestrictedArea.center, m_OnLadderState.RestrictedArea.extents * 2);
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetComponent<Collider2D>().bounds.center, .5f);
    }

    #endregion

    public bool IsState(MotorState state)
    {
        return m_MotorState == state;
    }

    public bool IsInLadderArea()
    {
        return m_OnLadderState.IsInLadderArea;
    }

    public bool IsInLadderTopArea()
    {
        return m_OnLadderState.IsInLadderArea && m_OnLadderState.AreaZone == LadderZone.Top;
    }

    public bool IsGrounded()
    {
        return m_Motor.Collisions.Below;
    }

    public bool IsInAir()
    {
        return !m_Motor.Collisions.Below && !m_Motor.Collisions.Left && !m_Motor.Collisions.Right; //IsState(MotorState.Jumping) || IsState(MotorState.Falling);
    }

    public bool IsAgainstWall()
    {
        return m_WallJumpSettings.CanWallJump && (m_Motor.Collisions.Left || m_Motor.Collisions.Right);
    }

    public bool CanAirJump()
    {
        if (CanAirJumpFunc != null)
        {
            return CanAirJumpFunc();
        }
        else
        {
            return m_AirJumpCounter < m_JumpSettings.AirJumpAllowed;
        }
    }

    public void ChangeDashModule(BaseDashModule module, bool disableDashingState = false)
    {
        if (module != null)
        {
            DashModule = module;

            if (disableDashingState)
            {
                changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
            }
        }
        else
        {
            DashModule = null;
            changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
        }
    }
    
    public void ChangeActionModule(BaseActionModule module, bool disableActionState = false)
    {
        if (module != null)
        {
            ActionModule = module;

            if (disableActionState)
            {
                changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
            }
        }
        else
        {
            ActionModule = null;
            changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
        }
    }

    public void LadderAreaEnter(Bounds area, float topAreaHeight, float bottomAreaHeight)
    {
        m_OnLadderState.IsInLadderArea = true;
        m_OnLadderState.Area = area;

        m_OnLadderState.TopArea = new Bounds(
            new Vector3(area.center.x, area.center.y + area.extents.y - topAreaHeight / 2, 0),
            new Vector3(area.size.x, topAreaHeight, 100)
        );

        m_OnLadderState.BottomArea = new Bounds(
            new Vector3(area.center.x, area.center.y - area.extents.y + bottomAreaHeight / 2, 0),
            new Vector3(area.size.x, bottomAreaHeight, 100)
        );
    }

    public void LadderAreaExit()
    {
        m_OnLadderState.IsInLadderArea = false;

        m_OnLadderState.Area = new Bounds(Vector3.zero, Vector3.zero);
        m_OnLadderState.TopArea = new Bounds(Vector3.zero, Vector3.zero);
        m_OnLadderState.BottomArea = new Bounds(Vector3.zero, Vector3.zero);

        if (IsState(MotorState.OnLadder))
        {
            exitLadderState();
        }
    }

    public void SetLadderRestrictedArea(Bounds b, bool isTopIgnored = false)
    {
        m_OnLadderState.HasRestrictedArea = true;

        m_OnLadderState.RestrictedArea = b;
        m_OnLadderState.RestrictedAreaTopRight = b.center + b.extents;
        m_OnLadderState.RestrictedAreaBottomLeft = b.center - b.extents;

        if (isTopIgnored)
        {
            m_OnLadderState.RestrictedAreaTopRight.y = Mathf.Infinity;
        }
    }

    public void SetLadderZone(LadderZone zone)
    {
        m_OnLadderState.AreaZone = zone;
    }

    public void ClearLadderRestrictedArea()
    {
        m_OnLadderState.HasRestrictedArea = false;
    }

    public bool IsRestrictedOnLadder()
    {
        return m_OnLadderState.HasRestrictedArea;
    }

    public void Init()
    {
        // S = V0 * t + a * t^2 * 0.5
        // h = V0 * t + g * t^2 * 0.5
        // h = g * t^2 * 0.5
        // g = h / (t^2*0.5)

        m_Gravity = -m_JumpSettings.MaxHeight    / (m_JumpSettings.TimeToApex * m_JumpSettings.TimeToApex * 0.5f);
        m_MaxJumpSpeed = Mathf.Abs(m_Gravity) * m_JumpSettings.TimeToApex;
        m_MinJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(m_Gravity) * m_JumpSettings.MinHeight);
        m_AirJumpCounter = 0;

        m_Motor.OnMotorCollisionEnter2D += onMotorCollisionEnter2D;
        m_Motor.OnMotorCollisionStay2D += onMotorCollisionStay2D;
    }

    public void _Update(float timeStep)
    {
        updateTimers(timeStep);

        updateState(timeStep);

        // read input from input driver
        Vector2 input = new Vector2(m_InputDriver.Horizontal, m_InputDriver.Vertical);

        int rawInputX = 0;
        if (input.x > 0.0f)
            rawInputX = 1;
        else if (input.x < 0.0f)
            rawInputX = -1;

        // check which side of character is collided
        int wallDirX = m_Motor.Collisions.Right ? 1 : -1;
        
        // check if want dashing
        if (m_InputDriver.Dash)
        {
            startDash(rawInputX, timeStep);
        }

        // check if want climbing ladder
        if (IsInLadderArea())
        {
            if (IsInLadderTopArea())
            {
                if (input.y < 0.0f)
                {
                    enterLadderState();
                }
            }
            else
            {
                if (input.y > 0.0f)
                {
                    enterLadderState();
                }
            }
        }

        // dashing state
        if (IsState(MotorState.Dashing))
        {
            m_Velocity.x = m_DashState.DashDir * m_DashState.GetDashSpeed(); //getDashSpeed();

            if (!IsGrounded() && DashModule.UseGravity)
                m_Velocity.y = 0;

            if (DashModule.ChangeFacing)
            {
                FacingDirection = (int)Mathf.Sign(m_Velocity.x);
            }

            if (DashModule.UseCollision)
            {
                m_Motor.Move(m_Velocity * timeStep, false);
            }
            // teleport, if there is no obstacle on the target position -> teleport, or use collision to find the closest teleport position
            else
            {
                bool cannotTeleportTo = Physics2D.OverlapBox(
                    m_Motor.Collider2D.bounds.center + m_Velocity * timeStep, 
                    m_Motor.Collider2D.bounds.size, 
                    0.0f, 
                    m_Motor.Raycaster.CollisionLayer);

                if (!cannotTeleportTo)
                {
                    m_Motor.transform.Translate(m_Velocity * timeStep);
                }
                else
                {
                    m_Motor.Move(m_Velocity * timeStep, false);
                }
            }
        }

        // on custom action
        else if (IsState(MotorState.CustomAction))
        {
            //m_Velocity.x = m_ActionState.GetActionVelocity();

            //if (!IsGrounded() && DashModule.UseGravity)
            //    m_Velocity.y = 0;
        }

        // on ladder state
        else if (IsState(MotorState.OnLadder))
        {
            m_Velocity = input * m_MovementSettings.OnLadderSpeed;

            // jump if jump input is true
            if (m_InputDriver.Jump)
            {
                startJump(false, rawInputX, wallDirX);
            }

            if (m_Velocity.x != 0.0f)
                FacingDirection = (int)Mathf.Sign(m_Velocity.x);

            //m_Motor.Move(m_Velocity * timeStep, false);

            // dont do collision detection
            if (m_OnLadderState.HasRestrictedArea)
            {
                // outside right, moving right disallowed
                if (m_Motor.transform.position.x > m_OnLadderState.RestrictedAreaTopRight.x)
                {
                    if (m_Velocity.x > 0.0f)
                    {
                        m_Velocity.x = 0.0f;
                    }
                }

                // outside left, moving left disallowed
                if (m_Motor.transform.position.x < m_OnLadderState.RestrictedAreaBottomLeft.x)
                {
                    if (m_Velocity.x < 0.0f)
                    {
                        m_Velocity.x = 0.0f;
                    }
                }

                // outside up, moving up disallowed
                if (m_Motor.transform.position.y > m_OnLadderState.RestrictedAreaTopRight.y)
                {
                    if (m_Velocity.y > 0.0f)
                    {
                        m_Velocity.y = 0.0f;
                    }
                }

                // outside down, moving down disallowed
                if (m_Motor.transform.position.y < m_OnLadderState.RestrictedAreaBottomLeft.y)
                {
                    if (m_Velocity.y < 0.0f)
                    {
                        m_Velocity.y = 0.0f;
                    }
                }
            }
            /*
            var newPos = m_Motor.transform.position + m_Velocity * timeStep;

            if (m_OnLadderState.HasRestrictedArea)
            {

            }
            */
            m_Motor.transform.position += m_Velocity * timeStep;

            if (m_OnLadderState.HasRestrictedArea)
            {
                Vector2 pos = m_Motor.transform.position;
                pos.x = Mathf.Clamp(pos.x, m_OnLadderState.RestrictedAreaBottomLeft.x, m_OnLadderState.RestrictedAreaTopRight.x);
                pos.y = Mathf.Clamp(pos.y, m_OnLadderState.RestrictedAreaBottomLeft.y, m_OnLadderState.RestrictedAreaTopRight.y);

                // restricted in x axis
                if (pos.x != m_Motor.transform.position.x)
                {
                    pos.x = Mathf.Lerp(m_Motor.transform.position.x, pos.x, 0.25f);
                }

                // restricted in y axis
                if (pos.y != m_Motor.transform.position.y)
                {
                    pos.y = Mathf.Lerp(m_Motor.transform.position.y, pos.y, 0.25f);
                }

                m_Motor.transform.position = pos;
            }
        }
        
        else // other state
        {
            // fall through one way platform
            if (m_InputDriver.HoldingJump && input.y < 0.0f)
            {
                m_Motor.FallThrough();
            }

            // setup velocity.x based on input
            float targetVecX = input.x * m_MovementSettings.Speed;

            // smooth x direction motion
            if (IsGrounded())
            {
                m_Velocity.x = targetVecX;
                m_VelocityXSmoothing = targetVecX;
            }
            else
            {
                m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVecX, ref m_VelocityXSmoothing, m_MovementSettings.AccelerationTimeAirborne);
            }
            /*
            m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVecX, ref m_VelocityXSmoothing,
                (m_Motor.Collisions.Below) ? m_MovementSettings.AccelerationTimeGrounded : m_MovementSettings.AccelerationTimeAirborne);
            */

            // check wall sliding and jumping
            bool wallSliding = false;

            if (!IsGrounded())
            {
                if (IsAgainstWall())
                {
                    changeState(MotorState.WallSliding);

                    if (m_WallJumpSettings.CanWallJump)
                    {
                        m_AirJumpCounter = 0;
                        OnResetJumpCounter.Invoke(MotorState.WallSliding);
                        OnWallSliding.Invoke(m_Motor.Collisions.Left ? -1 : 1);
                    }

                    wallSliding = true;

                    if (m_Velocity.y < -m_WallJumpSettings.WallSlidingSpeedMax)
                    {
                        m_Velocity.y = -m_WallJumpSettings.WallSlidingSpeedMax;
                    }

                    // check if still sticking to wall
                    if (m_WallJumpSettings.WallStickTimer > 0.0f)
                    {
                        m_VelocityXSmoothing = 0;
                        m_Velocity.x = 0;

                        if (input.x != wallDirX && input.x != 0)
                        {
                            m_WallJumpSettings.WallStickTimer -= timeStep;
                        }
                        else
                        {
                            m_WallJumpSettings.WallStickTimer = m_WallJumpSettings.WallStickTime;
                        }
                    }
                    else
                    {
                        m_WallJumpSettings.WallStickTimer = m_WallJumpSettings.WallStickTime;
                    }
                }
            }

            // Reset gravity if collision happened in y axis
            if (m_Motor.Collisions.Above || m_Motor.Collisions.Below)
            {
                //Debug.Log("Reset Vec Y");
                m_Velocity.y = 0;
            }

            // jump if jump input is true
            if (m_InputDriver.Jump && input.y >= 0.0f)
            {
                startJump(wallSliding, rawInputX, wallDirX);
            }

            // variable jump height based on user input
            if (m_JumpSettings.HasVariableJumpHeight)
            {
                if (m_InputDriver.ReleaseJump && input.y >= 0.0f)
                {
                    if (m_Velocity.y > m_MinJumpSpeed)
                        m_Velocity.y = m_MinJumpSpeed;
                }
            }

            if (m_ApplyGravity)
            {
                m_Velocity.y += m_Gravity * timeStep;
            }

            if (wallSliding)
            {
                FacingDirection = (m_Motor.Collisions.Left) ? 1 : -1;
            }
            else
            {
                if (m_Velocity.x != 0.0f)
                    FacingDirection = (int)Mathf.Sign(m_Velocity.x);
            }

            m_Motor.Move(m_Velocity * timeStep, false);
        }

        // check ladder area
        if (IsInLadderArea())
        {
            if (m_OnLadderState.BottomArea.Contains(m_Motor.Collider2D.bounds.center))
            {
                m_OnLadderState.AreaZone = LadderZone.Bottom;
            }
            else if (m_OnLadderState.TopArea.Contains(m_Motor.Collider2D.bounds.center))
            {
                m_OnLadderState.AreaZone = LadderZone.Top;
            }
            else if (m_OnLadderState.Area.Contains(m_Motor.Collider2D.bounds.center))
            {
                m_OnLadderState.AreaZone = LadderZone.Middle;
            }
        }
    }


    private void updateTimers(float timeStep)
    {
        if (IsState(MotorState.Dashing))
        {
            m_DashState._Update(timeStep);
        }

        if (IsState(MotorState.CustomAction))
        {
            m_ActionState._Update(timeStep);
        }
    }

    private void updateState(float timeStep)
    {
        if (IsState(MotorState.Dashing))
        {
            OnDashStay(m_DashState.GetDashProgress());

            if (m_DashState.GetDashProgress() >= 1.0f)
            {
                endDash();
            }
        }

        if (IsState(MotorState.Dashing))
        {
            return;
        }

        if (IsState(MotorState.CustomAction))
        {
            OnActionStay(m_ActionState.GetActionProgress());

            if (m_ActionState.GetActionProgress() >= 1.0f)
            {
                endAction();
            }
        }

        if (IsState(MotorState.CustomAction))
        {
            return;
        }

        if (IsState(MotorState.Jumping))
        {
            if (m_Motor.Velocity.y < 0)
            {
                endJump();
            }
        }
        
        if (IsGrounded())
        {
            if (!IsState(MotorState.OnLadder))
            {
                changeState(MotorState.OnGround);
            }
        }
        else
        {
            if (IsState(MotorState.OnGround))
            {
                changeState(MotorState.Falling);
            }
        }

        if (IsState(MotorState.WallSliding))
        {
            if (!IsAgainstWall())
            {
                if (m_Motor.Velocity.y < 0.0f)
                {
                    changeState(MotorState.Falling);
                }
                else
                {
                    changeState(MotorState.Jumping);
                }
            }
        }
    }

    private void startJump(bool isWallJump, int rawInputX, int wallDirX)
    {
        bool success = false;
        if (isWallJump)
        {
            wallJump(rawInputX, wallDirX);
            success = true;
        }
        else
        {
            success = normalJump();
        }

        if (success)
        {
            changeState(MotorState.Jumping);
            
            OnJump.Invoke();
        }
    }

    private bool normalJump()
    {
        if (IsGrounded())
        {
            m_Velocity.y = m_MaxJumpSpeed;

            OnNormalJump.Invoke();

            return true;
        }
        else if (IsState(MotorState.OnLadder))
        {
            m_Velocity.y = m_JumpSettings.OnLadderJumpForce;

            OnLadderJump.Invoke();

            return true;
        }
        else if (CanAirJump())
        {
            m_Velocity.y = m_MaxJumpSpeed;
            m_AirJumpCounter++;

            OnAirJump.Invoke();

            return true;
        }
        else
        {
            return false;
        }
    }

    private void wallJump(int rawInputX, int wallDirX)
    {   
        bool climbing = wallDirX == rawInputX;
        Vector2 jumpVec;

        
        // climbing
        if (climbing || rawInputX == 0)
        {
            //Debug.Log("Climb");
            jumpVec.x = -wallDirX * m_WallJumpSettings.ClimbForce.x;
            jumpVec.y = m_WallJumpSettings.ClimbForce.y;
        }
        // jump leap
        else
        {
            //Debug.Log("Leap");
            jumpVec.x = -wallDirX * m_WallJumpSettings.LeapForce.x;
            jumpVec.y = m_WallJumpSettings.LeapForce.y;
        }

        OnWallJump.Invoke(jumpVec);

        m_Velocity = jumpVec;
    }

    // highest point reached, start falling
    private void endJump()
    {
        if (IsState(MotorState.Jumping))
        {
            changeState(MotorState.Falling);
        }
    }

    private void startDash(int rawInputX, float timeStep)
    {
        if (DashModule == null)
        {
            return;
        }

        if (IsState(MotorState.Dashing))
        {
            return;
        }

        if (DashModule.CanOnlyBeUsedOnGround)
        {
            if (!IsGrounded())
            {
                return;
            }
        }

        int dashDir = (rawInputX != 0) ? rawInputX : FacingDirection;
        if (!DashModule.CanDashToSlidingWall)
        {
            if (IsState(MotorState.WallSliding))
            {
                int wallDir = (m_Motor.Collisions.Right) ? 1 : -1;
                if (dashDir == wallDir)
                {
                    //Debug.Log("Dash Disallowed");
                    return;
                }
            }
        }

        if (DashModule.ChangeFacing)
        {
            FacingDirection = (int)Mathf.Sign(m_Velocity.x);
        }

        if (!IsGrounded() && DashModule.UseGravity)
            m_Velocity.y = 0;

        m_DashState.Start(dashDir, timeStep);

        OnDash.Invoke(dashDir);

        changeState(MotorState.Dashing);
    }

    private void endDash()
    {
        if (IsState(MotorState.Dashing))
        {
            // smooth out or sudden stop
            float vecX = m_DashState.DashDir * m_DashState.GetDashSpeed();
            m_VelocityXSmoothing = vecX;
            m_Velocity.x = vecX;
            changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
        }
    }

    private void startAction(int rawInputX)
    {
        if (ActionModule == null)
        {
            return;
        }

        if (IsState(MotorState.CustomAction))
        {
            return;
        }

        if (DashModule.CanOnlyBeUsedOnGround)
        {
            if (!IsGrounded())
            {
                return;
            }
        }

        int actionDir = (rawInputX != 0) ? rawInputX : FacingDirection;
        
        if (!ActionModule.CanUseToSlidingWall)
        {
            if (IsState(MotorState.WallSliding))
            {
                int wallDir = (m_Motor.Collisions.Right) ? 1 : -1;
                if (actionDir == wallDir)
                {
                    return;
                }
            }
        }

        m_ActionState.Start(actionDir);

        changeState(MotorState.CustomAction);
    }

    private void endAction()
    {
        if (IsState(MotorState.CustomAction))
        {
            // smooth out or sudden stop
            Vector2 vec = new Vector2(m_ActionState.ActionDir, 1) * m_ActionState.GetActionVelocity();
            m_VelocityXSmoothing = vec.x;
            m_Velocity = vec;
            changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
        }
    }

    private void enterLadderState()
    {
        m_Velocity.x = 0;
        m_Velocity.y = 0;

        changeState(MotorState.OnLadder);

        m_AirJumpCounter = 0;
        OnResetJumpCounter.Invoke(MotorState.OnLadder);
        m_ApplyGravity = false;
    }

    private void exitLadderState()
    {
        m_Velocity.y = 0;
        changeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
    }

    // change state and callback
    private void changeState(MotorState state)
    {
        if (m_MotorState == state)
        {
            return;
        }

        if (IsState(MotorState.OnLadder))
        {
            m_ApplyGravity = true;
        }

        // exit old state action
        if (IsState(MotorState.Jumping))
        {
            OnJumpEnd.Invoke();
        }

        if (IsState(MotorState.Dashing))
        {
            OnDashEnd.Invoke();
        }

        if (IsState(MotorState.WallSliding))
        {
            OnWallSlidingEnd.Invoke();
        }

        // set new state
        var prevState = m_MotorState;
        m_MotorState = state;

        if (IsState(MotorState.OnGround))
        {
            m_AirJumpCounter = 0;
            OnResetJumpCounter.Invoke(MotorState.OnGround);
            OnLanded.Invoke();
        }

        OnMotorStateChanged.Invoke(prevState, m_MotorState);
    }

    private void onMotorCollisionEnter2D(MotorCollision2D col)
    {
        if (col.IsSurface(MotorCollision2D.CollisionSurface.Ground))
        {
            onCollisionEnterGround();
        }

        if (col.IsSurface(MotorCollision2D.CollisionSurface.Ceiling))
        {
            onCollisionEnterCeiling();
        }

        if (col.IsSurface(MotorCollision2D.CollisionSurface.Left))
        {
            onCollisionEnterLeft();
        }

        if (col.IsSurface(MotorCollision2D.CollisionSurface.Right))
        {
            onCollisionEnterRight();
        }
    }

    private void onCollisionEnterGround()
    {
        //Debug.Log("Ground!");
    }

    private void onCollisionEnterCeiling()
    {
        //Debug.Log("Ceiliing!");
    }

    private void onCollisionEnterLeft()
    {
        //Debug.Log("Left!");
    }

    private void onCollisionEnterRight()
    {
        //Debug.Log("Right!");
    }

    private void onMotorCollisionStay2D(MotorCollision2D col)
    {
        if (col.IsSurface(MotorCollision2D.CollisionSurface.Ground))
        {
            onCollisionStayGround();
        }

        if (col.IsSurface(MotorCollision2D.CollisionSurface.Left))
        {
            onCollisionStayLeft();
        }

        if (col.IsSurface(MotorCollision2D.CollisionSurface.Right))
        {
            onCollisionStayRight();
        }
    }

    private void onCollisionStayGround()
    {
        //Debug.Log("Ground!");
        //m_AirJumpCounter = 0;
    }

    private void onCollisionStayLeft()
    {
        //Debug.Log("Left!");
        //if (m_WallJumpSettings.CanWallJump)
        //{
        //    m_AirJumpCounter = 0;
        //}
    }

    private void onCollisionStayRight()
    {
        //Debug.Log("Right!");
        //if (m_WallJumpSettings.CanWallJump)
        //{
        //    m_AirJumpCounter = 0;
        //}
    }
}
