using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -- BasicCharacterController2D
 * 
 *  Handles Gravity, Jump and Horizontal Movement
 */

namespace DYP
{
    [RequireComponent(typeof(CharacterMotor2D))]
    public class BasicMovementController2D : MonoBehaviour
    {
        [System.Serializable]
        class MovementSettings
        {
            public float Speed = 8;
            public float AccelerationTimeAirborne = .1f;
            public float AccelerationTimeGrounded = .1f;
        }

        [System.Serializable]
        class OnLadderSettings
        {
            public bool SnapToRestrictedArea = false;
            public bool ExitLadderOnGround = true;
            public float OnLadderSpeed = 4;
            public bool LockFacingToRight = true;
        }

        [System.Serializable]
        class Jump
        {
            public bool HasVariableJumpHeight = true;
            public int AirJumpAllowed = 1;
            public float MaxHeight = 3.5f;
            public float MinHeight = 1.0f;
            public float TimeToApex = .4f;
            public float OnLadderJumpForce = 0.4f;

            public float FallingJumpPaddingTime = 0.09f;
            public float WillJumpPaddingTime = 0.15f;
        }

        [System.Serializable]
        class WallInteraction
        {
            public bool CanWallSlide = true;
            public float WallStickTime = 0.15f;
            [HideInInspector]
            public float WallStickTimer = 0.0f;
            public float WallSlideSpeedLoss = 0.05f;
            public float WallSlidingSpeedMax = 2;

            public bool CanWallClimb = false;
            public float WallClimbSpeed = 2;

            public bool CanWallJump = true;
            public Vector2 ClimbForce = new Vector2(12, 16);
            public Vector2 OffForce = new Vector2(8, 15);
            public Vector2 LeapForce = new Vector2(18, 17);

            public bool CanGrabLedge = false;
            public float LedgeDetectionOffset = 0.1f;

            [HideInInspector]
            public int WallDirX = 0;
        }

        [System.Serializable]
        class InputBuffer
        {
            public Vector2 Input = Vector2.zero;

            public bool IsJumpPressed = false;
            public bool IsJumpHeld = false;
            public bool IsJumpReleased = false;
            public bool IsDashPressed = false;
            public bool IsDashHeld = false;
        }

        [System.Serializable]
        class Dash
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
        class CustomAction
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

        private CharacterMotor2D m_Motor;
        public Collider2D MotorCollider
        {
            get
            {
                return m_Motor.Collider2D;
            }
        }

        private BaseInputDriver m_InputDriver;
        private InputBuffer m_InputBuffer = new InputBuffer();

        [Header("Settings")]
        [SerializeField]
        private MovementSettings m_MovementSettings;

        [SerializeField]
        private OnLadderSettings m_OnLadderSettings;

        [SerializeField]
        private Jump m_Jump;

        [SerializeField]
        private WallInteraction m_WallInteraction;

        private MotorState m_MotorState;

        private float m_Gravity;
        private bool m_ApplyGravity = true;

        private float m_MaxJumpSpeed;
        private float m_MinJumpSpeed;

        private int m_AirJumpCounter = 0;
        private int m_WillJumpPaddingFrame = -1;
        private int m_FallingJumpPaddingFrame = -1;

        private float m_CurrentTimeStep = 0;

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
        private Dash m_Dash = new Dash();

        [SerializeField]
        private CustomAction m_CustomAction = new CustomAction();

        public BaseDashModule DashModule { get { return m_Dash.Module; } private set { m_Dash.Module = value; } }

        public BaseActionModule ActionModule { get { return m_CustomAction.Module; } private set { m_CustomAction.Module = value; } }

        private OnLadderState m_OnLadderState = new OnLadderState();

        [Header("State")]
        private Vector3 m_Velocity;
        public Vector3 InputVelocity { get { return m_Velocity; } }

        public float MovementSpeed { get { return m_MovementSettings.Speed; } set { m_MovementSettings.Speed = value; } }

        private float m_VelocityXSmoothing;

        // Action
        public event Action<MotorState, MotorState> OnMotorStateChanged = delegate { };

        public event System.Action OnJump = delegate { };                // on all jump! // OnEnterStateJump
        public event System.Action OnJumpEnd = delegate { };             // on jump -> falling  // OnLeaveStateJump

        public event System.Action OnNormalJump = delegate { };          // on ground jump
        public event System.Action OnLedgeJump = delegate { };           // on ledge jump
        public event System.Action OnLadderJump = delegate { };          // on ladder jump
        public event System.Action OnAirJump = delegate { };             // on air jump
        public event System.Action<Vector2> OnWallJump = delegate { };   // on wall jump

        public event System.Action<int> OnDash = delegate { };           // int represent dash direction
        public event System.Action<float> OnDashStay = delegate { };     // float represent action progress
        public event System.Action OnDashEnd = delegate { };

        public event System.Action<int> OnWallSliding = delegate { };    // int represnet wall direction: 1 -> right, -1 -> left
        public event System.Action OnWallSlidingEnd = delegate { };

        public event System.Action<int> OnLedgeGrabbing = delegate { };
        public event System.Action OnLedgeGrabbingEnd = delegate { };

        public event System.Action OnLanded = delegate { };              // on grounded

        public event System.Action<MotorState> OnResetJumpCounter = delegate { };
        public event System.Action<int> OnFacingFlip = delegate { };

        public event System.Action<int> OnAction = delegate { };
        public event System.Action<float> OnActionStay = delegate { };
        public event System.Action OnActionEnd = delegate { };

        // Condition
        public Func<bool> CanAirJumpFunc = null;

        #region Monobehaviour

        private void Reset()
        {
            m_Motor = GetComponent<CharacterMotor2D>();
        }

        private void Awake()
        {
            m_Motor = GetComponent<CharacterMotor2D>();
            m_InputDriver = GetComponent<BaseInputDriver>();

            if (m_InputDriver == null)
            {
                Debug.LogWarning("An InputDriver is needed for a BasicCharacterController2D");
            }
        }

        private void Start()
        {
            Init();
        }

        private void FixedUpdate()
        {
            _Update(Time.fixedDeltaTime);
        }

        private void Update()
        {
            readInput(Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
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

            // draw ledge grabbing gizmos
            if (m_Motor != null)
            {
                var boundingBox = MotorCollider.bounds;

                Vector2 origin = Vector2.zero;
                origin.y = boundingBox.max.y + m_WallInteraction.LedgeDetectionOffset;

                float distance = m_WallInteraction.LedgeDetectionOffset * 2;
                float speedY = -m_Velocity.y * m_CurrentTimeStep;
                distance = (speedY > distance) ? speedY : distance;

                Vector2 hitPoint = Vector2.zero;

                // right ledge line
                origin.x = boundingBox.max.x + m_WallInteraction.LedgeDetectionOffset;

                bool rightLedge = CheckIfAtLedge(1, ref hitPoint);

                Gizmos.color = (rightLedge) ? Color.blue : Color.red;
                Gizmos.DrawLine(origin, origin + Vector2.down * distance);

                if (rightLedge)
                {
                    Gizmos.DrawSphere(hitPoint, 0.03f);
                }


                // left ledge line
                origin.x = boundingBox.min.x - m_WallInteraction.LedgeDetectionOffset;

                bool leftLedge = CheckIfAtLedge(-1, ref hitPoint);

                Gizmos.color = (leftLedge) ? Color.blue : Color.red;
                Gizmos.DrawLine(origin, origin + Vector2.down * distance);

                if (leftLedge)
                {
                    Gizmos.DrawSphere(hitPoint, 0.03f);
                }
            }

            // draw center
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(GetComponent<Collider2D>().bounds.center, .5f);
        }

        #endregion

        public void SetFrozen(bool freeze)
        {
            if (freeze)
            {
                m_Velocity.x = 0;
                changeState(MotorState.Frozen);
            }
            else
            {
                if (IsOnGround())
                {
                    changeState(MotorState.OnGround);
                }
                else
                {
                    changeState(MotorState.Falling);
                }
            }
        }

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

        public bool IsOnGround()
        {
            return m_Motor.Collisions.Below;
        }

        public bool IsInAir()
        {
            return !m_Motor.Collisions.Below && !m_Motor.Collisions.Left && !m_Motor.Collisions.Right; //IsState(MotorState.Jumping) || IsState(MotorState.Falling);
        }

        public bool IsAgainstWall()
        {
            if (m_Motor.Collisions.Left)
            {
                float leftWallAngle = Vector2.Angle(m_Motor.Collisions.LeftNormal, Vector2.right);
                if (leftWallAngle < 0.01f)
                {
                    return true;
                }
            }

            if (m_Motor.Collisions.Right)
            {
                float rightWallAngle = Vector2.Angle(m_Motor.Collisions.RightNormal, Vector2.left);
                if (rightWallAngle < 0.01f)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckIfAtLedge(int wallDirX, ref Vector2 ledgePoint)
        {
            // first raycast down, then check overlap
            var boundingBox = MotorCollider.bounds;

            Vector2 origin = Vector2.zero;
            origin.y = boundingBox.max.y + m_WallInteraction.LedgeDetectionOffset;

            // right wall
            if (wallDirX == 1)
            {
                origin.x = boundingBox.max.x + m_WallInteraction.LedgeDetectionOffset;
            }
            // left wall
            else if (wallDirX == -1)
            {
                origin.x = boundingBox.min.x - m_WallInteraction.LedgeDetectionOffset;
            }

            float distance = m_WallInteraction.LedgeDetectionOffset * 2;
            float speedY = -m_Velocity.y * m_CurrentTimeStep;
            distance = (speedY > distance) ? speedY : distance;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, m_Motor.Raycaster.CollisionLayer);

            ledgePoint = hit.point;

            if (hit.collider != null)
            {
                Bounds overlapBox = new Bounds(
                    new Vector3(ledgePoint.x, ledgePoint.y) + Vector3.up * m_WallInteraction.LedgeDetectionOffset,
                    Vector3.one * m_WallInteraction.LedgeDetectionOffset);

                Collider2D col = Physics2D.OverlapArea(overlapBox.min, overlapBox.max);
                return (col == null);
            }
            else
            {
                return false;
            }
        }

        public bool CanAirJump()
        {
            if (CanAirJumpFunc != null)
            {
                return CanAirJumpFunc();
            }
            else
            {
                return m_AirJumpCounter < m_Jump.AirJumpAllowed;
            }
        }

        public void ChangeDashModule(BaseDashModule module, bool disableDashingState = false)
        {
            if (module != null)
            {
                DashModule = module;

                if (disableDashingState)
                {
                    changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
                }
            }
            else
            {
                DashModule = null;
                changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
            }
        }

        public void ChangeActionModule(BaseActionModule module, bool disableActionState = false)
        {
            if (module != null)
            {
                ActionModule = module;

                if (disableActionState)
                {
                    changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
                }
            }
            else
            {
                ActionModule = null;
                changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
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

            m_Gravity = -m_Jump.MaxHeight / (m_Jump.TimeToApex * m_Jump.TimeToApex * 0.5f);
            m_MaxJumpSpeed = Mathf.Abs(m_Gravity) * m_Jump.TimeToApex;
            m_MinJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(m_Gravity) * m_Jump.MinHeight);
            m_AirJumpCounter = 0;

            m_Motor.OnMotorCollisionEnter2D += onMotorCollisionEnter2D;
            m_Motor.OnMotorCollisionStay2D += onMotorCollisionStay2D;
        }

        private void readInput(float timeStep)
        {
            // Update input buffer
            m_InputBuffer.Input = new Vector2(m_InputDriver.Horizontal, m_InputDriver.Vertical);

            m_InputBuffer.IsJumpPressed = m_InputDriver.Jump;

            if (m_InputBuffer.IsJumpPressed)
            {
                m_WillJumpPaddingFrame = calculateFramesFromTime(m_Jump.WillJumpPaddingTime, timeStep);
            }

            m_InputBuffer.IsJumpHeld = m_InputDriver.HoldingJump;
            m_InputBuffer.IsJumpReleased = m_InputDriver.ReleaseJump;

            m_InputBuffer.IsDashPressed = m_InputDriver.Dash;
            m_InputBuffer.IsDashHeld = m_InputDriver.HoldingDash;
        }

        public void _Update(float timeStep)
        {
            m_CurrentTimeStep = timeStep;

            updateTimers(timeStep);

            updateState(timeStep);

            // check padding frame
            if (m_WillJumpPaddingFrame >= 0)
            {
                m_InputBuffer.IsJumpPressed = true;
            }

            // read input from input driver
            Vector2 input = m_InputBuffer.Input;

            Vector2Int rawInput = Vector2Int.zero;

            if (input.x > 0.0f)
                rawInput.x = 1;
            else if (input.x < 0.0f)
                rawInput.x = -1;

            if (input.y > 0.0f)
                rawInput.y = 1;
            else if (input.y < 0.0f)
                rawInput.y = -1;

            // check which side of character is collided
            int wallDirX = 0;

            if (m_Motor.Collisions.Right)
            {
                wallDirX = 1;
            }
            else if (m_Motor.Collisions.Left)
            {
                wallDirX = -1;
            }

            m_WallInteraction.WallDirX = wallDirX;

            // check if want dashing
            if (m_InputBuffer.IsDashPressed)
            {
                startDash(rawInput.x, timeStep);
            }

            // check if want climbing ladder
            if (IsInLadderArea())
            {
                if (IsInLadderTopArea())
                {
                    if (rawInput.y < 0)
                    {
                        enterLadderState();
                    }
                }
                else
                {
                    if (rawInput.y > 0)
                    {
                        enterLadderState();
                    }
                }

            }

            // dashing state
            if (IsState(MotorState.Dashing))
            {
                m_Velocity.x = m_Dash.DashDir * m_Dash.GetDashSpeed(); //getDashSpeed();

                if (!IsOnGround() && DashModule.UseGravity)
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
                m_Velocity = input * m_OnLadderSettings.OnLadderSpeed;

                // jump if jump input is true
                if (m_InputBuffer.IsJumpPressed)
                {
                    startJump(rawInput, wallDirX);
                }

                if (m_OnLadderSettings.LockFacingToRight)
                {
                    FacingDirection = 1;
                }
                else
                {
                    if (m_Velocity.x != 0.0f)
                        FacingDirection = (int)Mathf.Sign(m_Velocity.x);
                }

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

                Vector2 targetPos = m_Motor.transform.position + m_Velocity * timeStep;

                Vector2 currPos = m_Motor.transform.position;

                // call Motor.Move to update collision info
                m_Motor.Move(targetPos - currPos);

                // actual updated position
                m_Motor.transform.position = targetPos;

                // Second pass check
                if (m_OnLadderState.HasRestrictedArea)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x, m_OnLadderState.RestrictedAreaBottomLeft.x, m_OnLadderState.RestrictedAreaTopRight.x);
                    targetPos.y = Mathf.Clamp(targetPos.y, m_OnLadderState.RestrictedAreaBottomLeft.y, m_OnLadderState.RestrictedAreaTopRight.y);

                    // restricted in x axis
                    if (targetPos.x != m_Motor.transform.position.x)
                    {
                        if (!m_OnLadderSettings.SnapToRestrictedArea)
                        {
                            targetPos.x = Mathf.Lerp(m_Motor.transform.position.x, targetPos.x, 0.25f);
                        }
                    }

                    // restricted in y axis
                    if (targetPos.y != m_Motor.transform.position.y)
                    {
                        if (!m_OnLadderSettings.SnapToRestrictedArea)
                        {
                            targetPos.y = Mathf.Lerp(m_Motor.transform.position.y, targetPos.y, 0.25f);
                        }
                    }

                    m_Motor.transform.position = targetPos;
                }
            }
            else if (IsState(MotorState.Frozen))
            {
                // Reset gravity if collision happened in y axis
                if (m_Motor.Collisions.Above)
                {
                    //Debug.Log("Reset Vec Y");
                    m_Velocity.y = 0;
                }
                else if (m_Motor.Collisions.Below)
                {
                    // falling downward
                    if (m_Velocity.y < 0.0f)
                    {
                        m_Velocity.y = 0;
                    }
                }

                if (m_ApplyGravity)
                {
                    float gravity = m_Gravity;
                    m_Velocity.y += gravity * timeStep;
                }
                m_Motor.Move(m_Velocity * timeStep, false);
            }
            else // other state
            {
                // fall through one way platform
                if (m_InputBuffer.IsJumpHeld && rawInput.y < 0)
                {
                    m_Motor.FallThrough();
                    changeState(MotorState.Falling);
                }

                // setup velocity.x based on input
                float targetVecX = input.x * m_MovementSettings.Speed;

                // smooth x direction motion
                if (IsOnGround())
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

                // check wall sticking and jumping
                bool isStickToWall = false;
                bool isGrabbingLedge = false;
                Vector2 ledgePos = Vector2.zero;

                if (IsAgainstWall())
                {
                    // ledge grabbing logic
                    if (m_WallInteraction.CanGrabLedge)
                    {
                        if (CheckIfAtLedge(wallDirX, ref ledgePos))
                        {
                            if (!IsState(MotorState.OnLedge))
                            {
                                if (m_Velocity.y < 0 && wallDirX == rawInput.x)
                                {
                                    isGrabbingLedge = true;
                                    m_Velocity.y = 0;

                                    float adjustY = ledgePos.y - MotorCollider.bounds.max.y;

                                    m_Motor.transform.position += Vector3.up * adjustY;
                                }
                            }
                            else
                            {
                                isGrabbingLedge = true;
                            }
                        }

                        if (isGrabbingLedge)
                        {
                            changeState(MotorState.OnLedge);

                            m_AirJumpCounter = 0;
                            OnResetJumpCounter.Invoke(MotorState.OnLedge);

                            // check if still sticking to wall
                            if (m_WallInteraction.WallStickTimer > 0.0f)
                            {
                                m_VelocityXSmoothing = 0;
                                m_Velocity.x = 0;

                                // leaving wall
                                if (rawInput.x == -wallDirX)
                                {
                                    m_WallInteraction.WallStickTimer -= timeStep;
                                }
                                // not leaving wall
                                else
                                {
                                    m_WallInteraction.WallStickTimer = m_WallInteraction.WallStickTime;
                                }
                            }
                            else
                            {
                                changeState(MotorState.Falling);
                                m_WallInteraction.WallStickTimer = m_WallInteraction.WallStickTime;
                            }
                        }

                    }

                    // wall sliding logic
                    if (!isGrabbingLedge && m_WallInteraction.CanWallSlide)
                    {
                        // is OnGround, press against wall and jump
                        if (IsOnGround())
                        {
                            if (!IsState(MotorState.WallSliding))
                            {
                                if (m_WallInteraction.CanWallClimb)
                                {
                                    if (rawInput.x == wallDirX && m_InputBuffer.IsJumpPressed)
                                    {
                                        isStickToWall = true;

                                        consumeJumpPressed();
                                    }
                                }
                            }
                        }
                        // is not OnGround, press against wall or was wallsliding
                        else
                        {
                            if (IsState(MotorState.WallSliding) || rawInput.x == wallDirX)
                            {
                                isStickToWall = true;
                            }
                        }

                        if (isStickToWall)
                        {
                            changeState(MotorState.WallSliding);

                            m_AirJumpCounter = 0;
                            OnResetJumpCounter.Invoke(MotorState.WallSliding);

                            // check if still sticking to wall
                            if (m_WallInteraction.WallStickTimer > 0.0f)
                            {
                                m_VelocityXSmoothing = 0;
                                m_Velocity.x = 0;

                                if (rawInput.x != wallDirX && rawInput.x != 0)
                                {
                                    m_WallInteraction.WallStickTimer -= timeStep;

                                    if (m_WallInteraction.WallStickTimer < 0.0f)
                                    {
                                        changeState(MotorState.Falling);
                                        m_WallInteraction.WallStickTimer = m_WallInteraction.WallStickTime;
                                    }
                                }
                                else
                                {
                                    m_WallInteraction.WallStickTimer = m_WallInteraction.WallStickTime;
                                }
                            }
                            else
                            {
                                changeState(MotorState.Falling);
                                m_WallInteraction.WallStickTimer = m_WallInteraction.WallStickTime;
                            }
                        }
                    }
                }

                // Reset gravity if collision happened in y axis
                if (m_Motor.Collisions.Above)
                {
                    //Debug.Log("Reset Vec Y");
                    m_Velocity.y = 0;
                }
                else if (m_Motor.Collisions.Below)
                {
                    // falling downward
                    if (m_Velocity.y < 0.0f)
                    {
                        m_Velocity.y = 0;
                    }
                }

                // jump if jump input is true
                if (m_InputBuffer.IsJumpPressed && rawInput.y >= 0)
                {
                    startJump(rawInput, wallDirX);
                }

                // variable jump height based on user input
                if (m_Jump.HasVariableJumpHeight)
                {
                    if (!m_InputBuffer.IsJumpHeld && rawInput.y >= 0)
                    {
                        if (m_Velocity.y > m_MinJumpSpeed)
                            m_Velocity.y = m_MinJumpSpeed;
                    }
                }

                if (m_ApplyGravity)
                {
                    float gravity = m_Gravity;
                    if (IsState(MotorState.WallSliding) && m_Velocity.y < 0)
                    {
                        gravity *= m_WallInteraction.WallSlideSpeedLoss;
                    }

                    m_Velocity.y += gravity * timeStep;
                }

                // control ledge grabbing speed
                if (isGrabbingLedge)
                {
                    if (IsState(MotorState.OnLedge))
                    {
                        if (m_Velocity.y < 0)
                        {
                            m_Velocity.y = 0;
                        }
                    }

                    FacingDirection = (m_Motor.Collisions.Right) ? 1 : -1;
                }
                // control wall sliding speed
                else if (isStickToWall)
                {
                    if (m_WallInteraction.CanWallClimb)
                    {
                        if (IsState(MotorState.WallSliding))
                        {
                            m_Velocity.y = input.y * m_WallInteraction.WallClimbSpeed;
                        }
                    }
                    else
                    {
                        if (m_Velocity.y < -m_WallInteraction.WallSlidingSpeedMax)
                        {
                            m_Velocity.y = -m_WallInteraction.WallSlidingSpeedMax;
                        }
                    }

                    FacingDirection = (m_Motor.Collisions.Right) ? 1 : -1;
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
                m_Dash._Update(timeStep);
            }

            if (IsState(MotorState.CustomAction))
            {
                m_CustomAction._Update(timeStep);
            }


            if (m_FallingJumpPaddingFrame >= 0) m_FallingJumpPaddingFrame--;
            if (m_WillJumpPaddingFrame >= 0) m_WillJumpPaddingFrame--;
        }

        private void updateState(float timeStep)
        {
            if (IsState(MotorState.Dashing))
            {
                OnDashStay(m_Dash.GetDashProgress());

                if (m_Dash.GetDashProgress() >= 1.0f)
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
                OnActionStay(m_CustomAction.GetActionProgress());

                if (m_CustomAction.GetActionProgress() >= 1.0f)
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

            if (IsOnGround())
            {
                if (IsState(MotorState.OnLadder))
                {
                    if (m_OnLadderSettings.ExitLadderOnGround)
                    {
                        if (!IsInLadderTopArea())
                        {
                            changeState(MotorState.OnGround);
                        }
                    }
                }
                else
                {
                    if (!IsState(MotorState.Frozen))
                    {
                        changeState(MotorState.OnGround);
                    }
                }
            }
            else
            {
                if (IsState(MotorState.OnGround))
                {
                    m_FallingJumpPaddingFrame = calculateFramesFromTime(m_Jump.FallingJumpPaddingTime, timeStep);

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

        private void startJump(Vector2Int rawInput, int wallDirX)
        {
            bool success = false;

            if (IsState(MotorState.OnLedge))
            {
                ledgeJump();
                success = true;
            }
            else if (IsState(MotorState.WallSliding))
            {
                if (m_WallInteraction.CanWallJump)
                {
                    wallJump(rawInput.x, wallDirX);
                    success = true;
                }
            }
            else
            {
                success = normalJump();
            }

            if (success)
            {
                consumeJumpPressed();

                changeState(MotorState.Jumping);

                OnJump.Invoke();
            }
        }

        private bool normalJump()
        {
            if (IsState(MotorState.OnGround))
            {
                m_Velocity.y = m_MaxJumpSpeed;

                OnNormalJump.Invoke();

                return true;
            }
            else if (IsState(MotorState.OnLadder))
            {
                m_Velocity.y = m_Jump.OnLadderJumpForce;

                OnLadderJump.Invoke();

                return true;
            }
            else if (m_FallingJumpPaddingFrame >= 0)
            {
                m_Velocity.y = m_MaxJumpSpeed;

                OnNormalJump.Invoke();

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

        private void ledgeJump()
        {
            m_Velocity.y = m_MaxJumpSpeed;

            OnLedgeJump.Invoke();
        }

        private void wallJump(int rawInputX, int wallDirX)
        {
            bool climbing = wallDirX == rawInputX;
            Vector2 jumpVec;


            // climbing
            if (climbing || rawInputX == 0)
            {
                jumpVec.x = -wallDirX * m_WallInteraction.ClimbForce.x;
                jumpVec.y = m_WallInteraction.ClimbForce.y;
            }
            // jump leap
            else
            {
                jumpVec.x = -wallDirX * m_WallInteraction.LeapForce.x;
                jumpVec.y = m_WallInteraction.LeapForce.y;
            }

            OnWallJump.Invoke(jumpVec);

            m_Velocity = jumpVec;
        }

        private void consumeJumpPressed()
        {
            m_InputBuffer.IsJumpPressed = false;
            m_FallingJumpPaddingFrame = -1;
            m_WillJumpPaddingFrame = -1;
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
                if (!IsOnGround())
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

            if (!IsOnGround() && DashModule.UseGravity)
                m_Velocity.y = 0;

            m_Dash.Start(dashDir, timeStep);

            OnDash.Invoke(dashDir);

            changeState(MotorState.Dashing);
        }

        private void endDash()
        {
            if (IsState(MotorState.Dashing))
            {
                // smooth out or sudden stop
                float vecX = m_Dash.DashDir * m_Dash.GetDashSpeed();
                m_VelocityXSmoothing = vecX;
                m_Velocity.x = vecX;
                changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
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
                if (!IsOnGround())
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

            m_CustomAction.Start(actionDir);

            changeState(MotorState.CustomAction);
        }

        private void endAction()
        {
            if (IsState(MotorState.CustomAction))
            {
                // smooth out or sudden stop
                Vector2 vec = new Vector2(m_CustomAction.ActionDir, 1) * m_CustomAction.GetActionVelocity();
                m_VelocityXSmoothing = vec.x;
                m_Velocity = vec;
                changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
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
            changeState(IsOnGround() ? MotorState.OnGround : MotorState.Falling);
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

            if (IsState(MotorState.OnLedge))
            {
                OnLedgeGrabbingEnd.Invoke();
            }

            // set new state
            var prevState = m_MotorState;
            m_MotorState = state;

            if (IsState(MotorState.OnGround))
            {
                m_AirJumpCounter = 0;
                OnResetJumpCounter.Invoke(MotorState.OnGround);

                if (prevState != MotorState.Frozen)
                {
                    OnLanded.Invoke();
                }
            }

            if (IsState(MotorState.OnLedge))
            {
                OnLedgeGrabbing.Invoke(m_WallInteraction.WallDirX);
            }

            if (IsState(MotorState.WallSliding))
            {
                OnWallSliding.Invoke(m_WallInteraction.WallDirX);
            }

            OnMotorStateChanged.Invoke(prevState, m_MotorState);
        }

        private int calculateFramesFromTime(float time, float timeStep)
        {
            return Mathf.RoundToInt(time / timeStep);
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
}
