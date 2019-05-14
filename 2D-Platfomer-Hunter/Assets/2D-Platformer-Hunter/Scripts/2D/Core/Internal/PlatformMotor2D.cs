using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -- Platform2D
 * 
 *  Collider Motor that move a box collider properly
 *  appropriate for platform controller
 *  
 */

namespace DYP
{
    // Hide in component menu
    [AddComponentMenu("")]
    [RequireComponent(typeof(Raycaster))]
    public class PlatformMotor2D : BaseMotor2D
    {

        struct PassengerInfo
        {
            public Transform Target;
            public Vector3 Velocity;
            public bool IsOnPlatform;

            public PassengerInfo(Transform _target, Vector3 _vec, bool _onPlat)
            {
                Target = _target;
                Velocity = _vec;
                IsOnPlatform = _onPlat;
            }
        }

        // Reference
        [Header("Reference")]
        private Raycaster m_Raycaster;

        [Header("Settings")]
        [SerializeField]
        private LayerMask m_PassengerLayer;

        private HashSet<int> m_MovedPassengerIDs;
        private List<PassengerInfo> m_MoveBeforePlatformPassengers;
        private List<PassengerInfo> m_MoveAfterPlatformPassengers;

        private Dictionary<int, BaseMotor2D> m_HistoryPassengers;

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

            m_MovedPassengerIDs = new HashSet<int>();
            m_MoveAfterPlatformPassengers = new List<PassengerInfo>();
            m_MoveBeforePlatformPassengers = new List<PassengerInfo>();

            m_HistoryPassengers = new Dictionary<int, BaseMotor2D>();
        }

        public void _Update(float timeStep)
        {

        }

        public override void Move(Vector3 velocity, bool onMoveMotor = false)
        {
            m_Raycaster.UpdateRaycastOrigins();

            calculatePassengersMovement(velocity);

            movePassengers(m_MoveBeforePlatformPassengers);

            transform.Translate(velocity);

            movePassengers(m_MoveAfterPlatformPassengers);
        }

        private void movePassengers(List<PassengerInfo> passengers)
        {
            foreach (var passenger in passengers)
            {
                int instanceId = passenger.Target.GetInstanceID();

                // has not been a passenger before, record
                if (!m_HistoryPassengers.ContainsKey(instanceId))
                {
                    m_HistoryPassengers.Add(instanceId, passenger.Target.GetComponent<BaseMotor2D>());
                }

                // if has Motor2D, use Motor2D.Move, else Translate
                var motor = m_HistoryPassengers[instanceId];
                if (motor != null)
                {
                    motor.Move(passenger.Velocity, passenger.IsOnPlatform);
                }
                else
                {
                    passenger.Target.Translate(passenger.Velocity);
                }
            }
        }

        private void calculatePassengersMovement(Vector3 velocity)
        {
            m_MovedPassengerIDs.Clear();
            m_MoveAfterPlatformPassengers.Clear();
            m_MoveBeforePlatformPassengers.Clear();

            float directionX = Mathf.Sign(velocity.x);
            float directionY = Mathf.Sign(velocity.y);

            // vertical movement, move transform
            if (velocity.y != 0)
            {
                float rayLength = Mathf.Abs(velocity.y) + Raycaster.c_SkinWidth;

                for (int i = 0; i < m_Raycaster.VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = (directionY == -1) ? m_Raycaster.Origins.BottomLeft : m_Raycaster.Origins.TopLeft;
                    rayOrigin += Vector2.right * (m_Raycaster.VerticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_PassengerLayer);

                    Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.blue);
                    Debug.DrawRay(rayOrigin + Vector2.right * 0.1f, -Vector3.right * 0.2f, Color.red);

                    if (hit && hit.distance != 0)
                    {
                        if (!m_MovedPassengerIDs.Contains(hit.transform.GetInstanceID()))
                        {
                            m_MovedPassengerIDs.Add(hit.transform.GetInstanceID());

                            // if moving down and collide with object, ignored!
                            if (directionY == 1)
                            {
                                float pushX = velocity.x;
                                float pushY = velocity.y - (hit.distance - Raycaster.c_SkinWidth) * directionY;

                                Vector3 move = new Vector3(pushX, pushY);
                                m_MoveBeforePlatformPassengers.Add(new PassengerInfo(hit.transform, move, directionY == 1));
                                //hit.transform.Translate(new Vector3(pushX, pushY));
                                //Debug.Log("Plat V Move");
                            }
                        }
                    }
                }
            }

            // Passenger on top of a dowarding platform or a horizontal movment platform
            if (directionY == -1 || (velocity.y == 0 && velocity.x != 0))
            {
                float rayLength = Raycaster.c_SkinWidth * 2;

                for (int i = 0; i < m_Raycaster.VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = m_Raycaster.Origins.TopLeft +
                                        Vector2.right * (m_Raycaster.VerticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_PassengerLayer);

                    Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.blue);

                    if (hit && hit.distance != 0)
                    {
                        if (!m_MovedPassengerIDs.Contains(hit.transform.GetInstanceID()))
                        {
                            m_MovedPassengerIDs.Add(hit.transform.GetInstanceID());
                            float pushX = velocity.x;
                            float pushY = velocity.y;

                            Vector3 move = new Vector3(pushX, pushY);
                            m_MoveAfterPlatformPassengers.Add(new PassengerInfo(hit.transform, move, true));
                            //hit.transform.Translate(new Vector3(pushX, pushY));

                            //Debug.Log("Plat H/D Move");
                        }
                    }
                }
            }


            // horizontal movement, push transform away
            if (!gameObject.CompareTag("OneWayPlatform"))
            {
                if (velocity.x != 0)
                {
                    float rayLength = Mathf.Abs(velocity.x) + Raycaster.c_SkinWidth;

                    for (int i = 0; i < m_Raycaster.HorizontalRayCount; i++)
                    {
                        Vector2 rayOrigin = (directionX == -1) ? m_Raycaster.Origins.BottomLeft : m_Raycaster.Origins.BottomRight;
                        rayOrigin += Vector2.up * (m_Raycaster.HorizontalRaySpacing * i);
                        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_PassengerLayer);

                        Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.blue);

                        if (hit && hit.distance != 0)
                        {
                            if (!m_MovedPassengerIDs.Contains(hit.transform.GetInstanceID()))
                            {
                                m_MovedPassengerIDs.Add(hit.transform.GetInstanceID());
                                float pushX = velocity.x - (hit.distance - 2 * Raycaster.c_SkinWidth) * directionX;
                                float pushY = -Raycaster.c_SkinWidth;

                                Vector3 move = new Vector3(pushX, pushY);
                                m_MoveBeforePlatformPassengers.Add(new PassengerInfo(hit.transform, move, false));
                                //hit.transform.Translate(new Vector3(pushX, pushY));

                                //Debug.Log("Push");
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
