using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [RequireComponent(typeof(Waypoints2D))]
    public class WaypointInputDriver : MonoBehaviour
    {
        [SerializeField]
        private Waypoints2D m_Waypoints2D;

        [SerializeField]
        private PlatformController2D m_Controller;

        [SerializeField]
        private bool m_IsCyclic = true;
        public bool IsCyclic { get { return m_IsCyclic; } }

        [SerializeField]
        private float m_WaitTime = 0.5f;
        public float WaitTime { get { return m_WaitTime; } }

        public Action<int> OnReachPoint = delegate { };

        private int m_PrevPointIndex = 0;
        private int m_NextPointIndex = 1;
        private Vector2 m_CurrDirection;

        private void Reset()
        {
            m_Waypoints2D = GetComponent<Waypoints2D>();
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            float timeStep = Time.deltaTime;

            var curr = new Vector2() { x = transform.position.x, y = transform.position.y };
            var next = getPoint(m_NextPointIndex);
            var dir = next - curr;

            // reach new point
            if (dir.sqrMagnitude < 0.05f * m_Controller.MovementSpeed)
            {
                int start = m_NextPointIndex;
                int end = m_NextPointIndex + 1;

                if (IsCyclic)
                {
                    end %= m_Waypoints2D.Count;
                }
                else
                {
                    end %= m_Waypoints2D.Count * 2 - 1;
                }

                OnReachPoint.Invoke(start);

                setPath(start, end);
            }

            float speedRatio = 1;

            if (dir.sqrMagnitude > Mathf.Pow(m_Controller.MovementSpeed * timeStep, 2))
            {
                speedRatio = Vector2.Distance(curr, next) / (m_Controller.MovementSpeed * timeStep);
            }

            m_CurrDirection = dir.normalized;
            Vector2 input = m_CurrDirection * speedRatio;

            if (input.sqrMagnitude > 1.0f)
            {
                input.Normalize();
            }

            m_Controller.InputMovement(new Vector2(input.x, input.y));
        }

        public void Init()
        {
            setPath(0, 1);
        }

        private void setPath(int start, int end)
        {
            m_PrevPointIndex = start;
            m_NextPointIndex = end;

            var prevPoint = getPoint(m_PrevPointIndex);
            var nextPoint = getPoint(m_NextPointIndex);

            m_CurrDirection = (nextPoint - prevPoint).normalized;
        }

        private Vector2 getPoint(int index)
        {
            if (IsCyclic || index < m_Waypoints2D.Count)
            {
                return m_Waypoints2D.At(index);
            }
            else
            {
                int offset = index - m_Waypoints2D.Count + 1;
                int retIndex = m_Waypoints2D.Count - offset - 1;
                return m_Waypoints2D.At(retIndex);
            }
        }
    }
}
