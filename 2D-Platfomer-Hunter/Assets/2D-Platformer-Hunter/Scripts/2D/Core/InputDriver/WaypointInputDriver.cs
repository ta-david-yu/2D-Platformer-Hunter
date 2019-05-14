using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [RequireComponent(typeof(Waypoints2D))]
    public class WaypointInputDriver : BaseInputDriver
    {
        private Waypoints2D m_Waypoints2D;

        [SerializeField]
        private bool m_IsCyclic = true;
        public bool IsCyclic { get { return m_IsCyclic; } }

        [SerializeField]
        private float m_WaitTime = 0.5f;
        public float WaitTime { get { return m_WaitTime; } }

        [SerializeField]
        private float m_Speed = 2.0f;
        public float Speed { get { return m_Speed; } }

        private int m_PrevPointIndex = 0;
        private int m_NextPointIndex = 1;
        private float m_CurrPathLength;
        private Vector2 m_CurrDirection;
        private float m_Progress = 0.0f;

        private void Awake()
        {
            m_Waypoints2D = GetComponent<Waypoints2D>();
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            UpdateInput(Time.deltaTime);
        }

        public void Init()
        {
            setPath(0, 1);
        }

        public override void UpdateInput(float timeStep)
        {
            m_Progress += timeStep * Speed / m_CurrPathLength;

            m_Progress = Mathf.Clamp01(m_Progress);

            var prev = getPoint(m_PrevPointIndex);
            var next = getPoint(m_NextPointIndex);

            Vector2 newPos = Vector2.Lerp(prev, next, m_Progress);

            // reach new point
            if (m_Progress >= 1.0f)
            {
                newPos = next;

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

                setPath(start, end);
            }

            Vector2 input = newPos - new Vector2(transform.position.x, transform.position.y);

            Vertical = input.y;
            Horizontal = input.x;
        }

        private void setPath(int start, int end)
        {
            m_PrevPointIndex = start;
            m_NextPointIndex = end;

            var prevPoint = getPoint(m_PrevPointIndex);
            var nextPoint = getPoint(m_NextPointIndex);

            m_CurrPathLength = Vector2.Distance(prevPoint, nextPoint);
            m_CurrDirection = (nextPoint - prevPoint).normalized;

            m_Progress = 0.0f;
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
