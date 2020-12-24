using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -- PlatformController2D
 * 
 *  Handles Down to Top platform, Passenger moving
 */
namespace DYP
{
    [RequireComponent(typeof(PlatformMotor2D))]
    public class PlatformController2D : MonoBehaviour, IHas2DAxisMovement
    {
        [System.Serializable]
        class MovementSettings
        {
            public float BaseSpeed = 1;
            public float SpeedMultiplayer = 1;
            public float Speed { get { return BaseSpeed * SpeedMultiplayer; } }
        }

        [Header("Reference")]
        [SerializeField]
        private PlatformMotor2D m_Motor;

        [Header("Settings")]

        [SerializeField]
        private MovementSettings m_Movement = new MovementSettings();

        public float MovementSpeed => m_Movement.Speed;

        private Vector2 m_MovementInputBuffer;

        #region Monobehaviour

        private void Reset()
        {
            m_Motor = GetComponent<PlatformMotor2D>();
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

        #endregion

        public void InputMovement(Vector2 axis)
        {
            m_MovementInputBuffer = axis;
        }

        public void Init()
        {
        }

        public void _Update(float timeStep)
        {
            Vector3 velocity = new Vector3(m_MovementInputBuffer.x, m_MovementInputBuffer.y, 0) * m_Movement.Speed;

            m_Motor.Move(velocity * timeStep);
        }
    }
}
