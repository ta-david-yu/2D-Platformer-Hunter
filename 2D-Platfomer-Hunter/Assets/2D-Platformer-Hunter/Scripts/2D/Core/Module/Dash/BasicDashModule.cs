using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [CreateAssetMenuAttribute(fileName = "BasicDashModule", menuName = "MovementModule/DashModule/Basic")]
    public class BasicDashModule : BaseDashModule
    {
        [SerializeField]
        protected float m_DashDistance = 3.0f;
        public float DashDistance { get { return m_DashDistance; } }

        [SerializeField]
        protected bool m_UseCollision = true;
        public override bool UseCollision { get { return m_UseCollision; } }

        [SerializeField]
        protected bool m_ResetVelocityY = true;
        public override bool UseGravity { get { return m_ResetVelocityY; } }

        [SerializeField]
        protected bool m_ChangeFacing = true;
        public override bool ChangeFacing { get { return m_ChangeFacing; } }

        [SerializeField]
        protected bool m_CanDashToSlidingWall = false;
        public override bool CanDashToSlidingWall { get { return m_CanDashToSlidingWall; } }

        [SerializeField]
        protected bool m_CanOnlyBeUsedOnGround = false;
        public override bool CanOnlyBeUsedOnGround { get { return m_CanOnlyBeUsedOnGround; } }

        [SerializeField]
        protected EasingFunction.Ease m_DashEaseType = EasingFunction.Ease.Linear;
        public EasingFunction.Ease DashEaseType { get { return m_DashEaseType; } }

        public override float GetDashSpeed(float prevTimer, float currTimer)
        {
            var prevTvalue = prevTimer / DashTime;
            if (prevTvalue > 1.0f)
                prevTvalue = 1.0f;

            var tvalue = currTimer / DashTime;
            if (tvalue > 1.0f)
                tvalue = 1.0f;

            var prevXvalue = EasingFunction.GetEasingFunction(DashEaseType)(0, DashDistance, prevTvalue);
            var xvalue = EasingFunction.GetEasingFunction(DashEaseType)(0, DashDistance, tvalue);

            float displacment = xvalue - prevXvalue;
            float speed = 0.0f;

            if (displacment > 0.0f)
            {
                speed = displacment / ((tvalue - prevTvalue) * DashTime);
            }

            //var speed = EasingFunction.GetEasingFunctionDerivative(DashEaseType)
            //        (0, DashDistance, tvalue) / DashTime;

            return (speed < 0.0f) ? 0 : speed;
        }
    }
}