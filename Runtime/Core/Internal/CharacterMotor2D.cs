using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -- Motor2D
 * 
 *  Collider Motor that move a box collider properly
 *  appropriate for character controller
 *  
 */

namespace DYP
{
    // Hide in component menu
    [AddComponentMenu("")]
    [RequireComponent(typeof(Raycaster))]
    [RequireComponent(typeof(Collider2D))]
    public class CharacterMotor2D : BaseMotor2D
    {
        #region DataStructure

        [System.Serializable]
        public struct CollisionInfo
        {
            public bool Above, Below;
            public bool Left, Right;

            public Vector2 AboveNormal, BelowNormal;
            public Vector2 LeftNormal, RightNormal;

            public bool Prev_Above, Prev_Below;
            public bool Prev_Left, Prev_Right;

            public bool ClimbingSlope;
            public bool DescendingSlope;
            public bool OnSlope;
            public int SlopeDirection;

            public bool Prev_ClimbingSlope;
            public bool Prev_DescendingSlope;
            public int Prev_SlopeDirection;

            // slope bug: 27 ~ 30 angle
            public float Curr_SlopeAngle, Prev_SlopeAngle;

            public void Reset()
            {
                Prev_Above = Above;
                Prev_Below = Below;
                Prev_Left = Left;
                Prev_Right = Right;

                Prev_ClimbingSlope = ClimbingSlope;
                Prev_DescendingSlope = DescendingSlope;

                Prev_SlopeDirection = SlopeDirection;

                Above = Below = false;
                Left = Right = false;

                ClimbingSlope = false;
                DescendingSlope = false;

                Prev_SlopeAngle = Curr_SlopeAngle;
                Curr_SlopeAngle = 0;

                SlopeDirection = 0;
            }
        }
        #endregion

        // Reference
        [Header("Reference")]
        private Raycaster m_Raycaster;
        public Raycaster Raycaster { get { return m_Raycaster; } }

        public Collider2D Collider2D { get { return m_Raycaster.Collider; } }

        private CollisionInfo m_CollisionInfo;
        public CollisionInfo Collisions { get { return m_CollisionInfo; } }

        // Settings
        [Header("Settings")]
        [SerializeField]
        private float m_MaxClimbAngle = 80;
        [SerializeField]
        private float m_MaxDescendAngle = 80;

        // Actions
        public Action<MotorCollision2D> OnMotorCollisionEnter2D = delegate { };
        public Action<MotorCollision2D> OnMotorCollisionExit2D = delegate { };
        public Action<MotorCollision2D> OnMotorCollisionStay2D = delegate { };

        // State
        private int m_MovingDirection = 1;
        private bool m_IsFallingThrough = false;


        #region Monobehaviour

        private void Awake()
        {
            m_Raycaster = GetComponent<Raycaster>();
        }

        // Temporary Start
        private void Start()
        {
            Init();
        }

        // Temporary Update
        private void Update()
        {
            _Update(Time.deltaTime);
        }

        #endregion

        #region MainFunction

        public void Init()
        {
            m_Raycaster.Init();
            //calculateRaySpacing();
            Velocity = Vector3.zero;
            Raw_Velocity = Vector3.zero;
            m_MovingDirection = 1;
        }

        public void _Update(float timeStep)
        {

        }

        public override void Move(Vector3 velocity, bool isOnMovingMotor = false)
        {
            m_Raycaster.UpdateRaycastOrigins();
            //updateRaycastOrigins();

            m_CollisionInfo.Reset();

            Raw_Velocity = velocity;

            velocity += ExternalForce;

            if (velocity.x != 0)
            {
                m_MovingDirection = (int)(Mathf.Sign(velocity.x));
            }

            if (velocity.y < 0)
            {
                descendSlope(ref velocity);
            }

            horizontalCollision(ref velocity);

            if (velocity.y != 0)
            {
                verticalCollision(ref velocity);
            }

            Velocity = velocity;

            /*
            var body = GetComponent<Rigidbody2D>();
            var vec3 = new Vector3(body.position.x, body.position.y, 0);
            body.MovePosition(vec3 + velocity);
            */
            movePosition(velocity);

            if (isOnMovingMotor)
            {
                setBelow(true, Vector2.up);
            }

            onCollisionEnterCallback();
            onCollisionExitCallback();
            onCollisionStayCallback();

            resetState();
        }

        private void movePosition(Vector3 velocity)
        {
            transform.Translate(velocity);
        }

        public void FallThrough()
        {
            m_IsFallingThrough = true;
            ExternalForce += -Vector3.up * Raycaster.c_SkinWidth;
        }

        private void resetState()
        {
            m_IsFallingThrough = false;
            ExternalForce = Vector3.zero;
        }

        private void horizontalCollision(ref Vector3 velocity)
        {
            float directionX = m_MovingDirection;
            float rayLength = Mathf.Abs(velocity.x) + Raycaster.c_SkinWidth;

            if (Mathf.Abs(velocity.x) < Raycaster.c_SkinWidth)
            {
                rayLength = 2 * Raycaster.c_SkinWidth;
            }

            for (int i = 0; i < m_Raycaster.HorizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? m_Raycaster.Origins.BottomLeft : m_Raycaster.Origins.BottomRight;

                rayOrigin += Vector2.up * (m_Raycaster.HorizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_Raycaster.CollisionLayer);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.yellow);

                if (hit)
                {
                    if (hit.distance == 0)
                    {
                        continue;
                    }

                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    // climbing a slope
                    if (i == 0 && slopeAngle <= m_MaxClimbAngle)
                    {
                        // if descending, reset velocity. Since descendSlope has changed velocity.x
                        if (m_CollisionInfo.DescendingSlope)
                        {
                            m_CollisionInfo.DescendingSlope = false;
                            velocity = Raw_Velocity;
                        }

                        float distanceToSlopeStart = 0;
                        // its a new slope
                        if (slopeAngle != m_CollisionInfo.Prev_SlopeAngle)
                        {
                            distanceToSlopeStart = hit.distance - Raycaster.c_SkinWidth;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }
                        climbSlope(ref velocity, slopeAngle, hit.normal);

                        velocity.x += distanceToSlopeStart * directionX;
                    }

                    // only check other raycasts if not climbing
                    if (!m_CollisionInfo.ClimbingSlope || slopeAngle > m_MaxClimbAngle)
                    {
                        if (hit.collider.CompareTag("OneWayPlatform"))
                        {
                            continue;
                        }

                        // change velocity x so it stops at the hit point
                        velocity.x = (hit.distance - Raycaster.c_SkinWidth) * directionX;
                        rayLength = hit.distance;

                        if (m_CollisionInfo.ClimbingSlope)
                        {
                            velocity.y =
                                Mathf.Tan(m_CollisionInfo.Curr_SlopeAngle * Mathf.Deg2Rad) *
                                Mathf.Abs(velocity.x);
                        }

                        setLeft(directionX == -1, hit.normal);
                        setRight(directionX == 1, hit.normal);
                    }
                }
            }
        }

        private void verticalCollision(ref Vector3 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + Raycaster.c_SkinWidth;

            for (int i = 0; i < m_Raycaster.VerticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? m_Raycaster.Origins.BottomLeft : m_Raycaster.Origins.TopLeft;

                // offset horizontal direction to precise detection
                rayOrigin += Vector2.right * (m_Raycaster.VerticalRaySpacing * i + velocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_Raycaster.CollisionLayer);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.yellow);

                if (hit)
                {
                    if (hit.collider.CompareTag("OneWayPlatform"))
                    {
                        if (directionY == 1 || hit.distance == 0)
                        {
                            continue;
                        }

                        if (m_IsFallingThrough)
                        {
                            continue;
                        }
                    }

                    // change velocity y so it stops at the hit point
                    velocity.y = (hit.distance - Raycaster.c_SkinWidth) * directionY;
                    rayLength = hit.distance;

                    if (m_CollisionInfo.ClimbingSlope)
                    {
                        velocity.x =
                            velocity.y / Mathf.Tan(m_CollisionInfo.Curr_SlopeAngle * Mathf.Deg2Rad);
                    }

                    setBelow(directionY == -1, hit.normal);
                    setAbove(directionY == 1, hit.normal);
                }
            }

            // to check if there is a new slope
            if (m_CollisionInfo.ClimbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                rayLength = Math.Abs(velocity.x) + Raycaster.c_SkinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? m_Raycaster.Origins.BottomLeft : m_Raycaster.Origins.BottomRight) +
                                    Vector2.up * velocity.y;

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_Raycaster.CollisionLayer);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != m_CollisionInfo.Curr_SlopeAngle)
                    {
                        //Debug.Log("New slope");
                        velocity.x = (hit.distance - Raycaster.c_SkinWidth) * directionX;
                        m_CollisionInfo.Curr_SlopeAngle = slopeAngle;
                    }
                }
            }
        }

        void climbSlope(ref Vector3 velocity, float slopeAngle, Vector2 slopeNormal)
        {
            float speed = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * speed;

            if (velocity.y <= climbVelocityY)
            {
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * speed * Mathf.Sign(velocity.x);

                velocity.y = climbVelocityY;

                m_CollisionInfo.Curr_SlopeAngle = slopeAngle;
                m_CollisionInfo.SlopeDirection = (int)Mathf.Sign(velocity.x);
                m_CollisionInfo.ClimbingSlope = true;
                setBelow(true, slopeNormal);
            }
        }

        void descendSlope(ref Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            Vector2 rayOrigin = (directionX == -1) ? m_Raycaster.Origins.BottomRight : m_Raycaster.Origins.BottomLeft;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, m_Raycaster.CollisionLayer);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (slopeAngle != 0 && slopeAngle <= m_MaxDescendAngle)
                {
                    // check if actually descending
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - Raycaster.c_SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad * Mathf.Abs(velocity.x)))
                        {
                            float speed = Mathf.Abs(velocity.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * speed;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * speed * Mathf.Sign(velocity.x);
                            velocity.y = -descendVelocityY;

                            m_CollisionInfo.Curr_SlopeAngle = slopeAngle;
                            m_CollisionInfo.SlopeDirection = -(int)Mathf.Sign(velocity.x);
                            m_CollisionInfo.DescendingSlope = true;
                            setBelow(true, hit.normal);
                        }
                    }
                }
            }
        }

        private void onCollisionEnterCallback()
        {
            MotorCollision2D col = new MotorCollision2D();

            col.Surface |= (!m_CollisionInfo.Prev_Above && m_CollisionInfo.Above) ? MotorCollision2D.CollisionSurface.Ceiling : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (!m_CollisionInfo.Prev_Below && m_CollisionInfo.Below) ? MotorCollision2D.CollisionSurface.Ground : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (!m_CollisionInfo.Prev_Left && m_CollisionInfo.Left) ? MotorCollision2D.CollisionSurface.Left : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (!m_CollisionInfo.Prev_Right && m_CollisionInfo.Right) ? MotorCollision2D.CollisionSurface.Right : MotorCollision2D.CollisionSurface.None;

            if (col.Surface != MotorCollision2D.CollisionSurface.None)
            {
                OnMotorCollisionEnter2D.Invoke(col);
            }
        }

        private void onCollisionExitCallback()
        {
            MotorCollision2D col = new MotorCollision2D();

            col.Surface |= (m_CollisionInfo.Prev_Above && !m_CollisionInfo.Above) ? MotorCollision2D.CollisionSurface.Ceiling : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Below && !m_CollisionInfo.Below) ? MotorCollision2D.CollisionSurface.Ground : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Left && !m_CollisionInfo.Left) ? MotorCollision2D.CollisionSurface.Left : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Right && !m_CollisionInfo.Right) ? MotorCollision2D.CollisionSurface.Right : MotorCollision2D.CollisionSurface.None;

            if (col.Surface != MotorCollision2D.CollisionSurface.None)
            {
                OnMotorCollisionExit2D.Invoke(col);
            }
        }

        private void onCollisionStayCallback()
        {
            MotorCollision2D col = new MotorCollision2D();

            col.Surface |= (m_CollisionInfo.Prev_Above && m_CollisionInfo.Above) ? MotorCollision2D.CollisionSurface.Ceiling : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Below && m_CollisionInfo.Below) ? MotorCollision2D.CollisionSurface.Ground : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Left && m_CollisionInfo.Left) ? MotorCollision2D.CollisionSurface.Left : MotorCollision2D.CollisionSurface.None;
            col.Surface |= (m_CollisionInfo.Prev_Right && m_CollisionInfo.Right) ? MotorCollision2D.CollisionSurface.Right : MotorCollision2D.CollisionSurface.None;

            if (col.Surface != MotorCollision2D.CollisionSurface.None)
            {
                OnMotorCollisionStay2D.Invoke(col);
            }
        }

        private void setBelow(bool value, Vector2 normal)
        {
            m_CollisionInfo.Below = value;
            m_CollisionInfo.BelowNormal = normal;
        }

        private void setAbove(bool value, Vector2 normal)
        {
            m_CollisionInfo.Above = value;
            m_CollisionInfo.AboveNormal = normal;
        }

        private void setLeft(bool value, Vector2 normal)
        {
            m_CollisionInfo.Left = value;
            m_CollisionInfo.LeftNormal = normal;
        }

        private void setRight(bool value, Vector2 normal)
        {
            m_CollisionInfo.Right = value;
            m_CollisionInfo.RightNormal = normal;
        }
        #endregion
    }
}
