using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public abstract class BaseDashModule : ScriptableObject
    {
        protected BasicMovementController2D m_Controller;

        [SerializeField]
        protected float m_DashTime = 0.15f;
        public float DashTime { get { return m_DashTime; } }

        public abstract bool UseCollision { get; }
        public abstract bool UseGravity { get; }
        public abstract bool ChangeFacing { get; }
        public abstract bool CanDashToSlidingWall { get; }
        public abstract bool CanOnlyBeUsedOnGround { get; }

        public abstract float GetDashSpeed(float prevTimer, float currTimer);

        public float GetDashProgress(float timer)
        {
            return timer / m_DashTime;
        }
    }
}
