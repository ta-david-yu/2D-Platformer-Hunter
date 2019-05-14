using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [System.Serializable]
    public abstract class BaseActionModule : ScriptableObject
    {
        [SerializeField]
        protected float m_ActionTime;
        public float ActionTime { get { return m_ActionTime; } }
        public abstract bool UseGravity { get; }
        public abstract bool CanUseToSlidingWall { get; }
        public abstract bool CanOnlyBeUsedOnGround { get; }


        public abstract Vector2 GetActionSpeed(float prevTimer, float currTimer);

        public float GetActionProgress(float timer)
        {
            return timer / m_ActionTime;
        }
    }
}
