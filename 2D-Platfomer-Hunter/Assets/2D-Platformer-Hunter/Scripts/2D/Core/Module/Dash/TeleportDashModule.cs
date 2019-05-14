using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [CreateAssetMenuAttribute(fileName = "TeleportDashModule", menuName = "MovementModule/DashModule/Teleport")]
    public class TeleportDashModule : BaseDashModule
    {
        public override bool CanDashToSlidingWall { get { return true; } }

        public override bool ChangeFacing { get { return true; } }

        public override bool UseGravity { get { return true; } }

        public override bool UseCollision { get { return false; } }

        public override bool CanOnlyBeUsedOnGround { get { return false; } }

        [SerializeField]
        private float m_TeleportDistance = 5.0f;

        [SerializeField]
        private float m_CastTime = 0.2f;

        private bool m_HasTeleported = false;

        public override float GetDashSpeed(float prevTimer, float currTimer)
        {
            var prevTvalue = prevTimer / DashTime;
            if (prevTvalue > 1.0f)
                prevTvalue = 1.0f;

            var tvalue = currTimer / DashTime;
            if (tvalue > 1.0f)
                tvalue = 1.0f;

            if (currTimer >= m_CastTime && !m_HasTeleported)
            {
                m_HasTeleported = true;
                return m_TeleportDistance / ((tvalue - prevTvalue) * DashTime);
            }
            else
            {
                return 0.0f;
            }
        }
    }
}
