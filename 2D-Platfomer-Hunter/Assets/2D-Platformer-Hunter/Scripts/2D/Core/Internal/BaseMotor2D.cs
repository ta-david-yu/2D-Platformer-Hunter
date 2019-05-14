using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public abstract class BaseMotor2D : MonoBehaviour
    {
        public bool IsOnMovingMotor { get; protected set; }
        public Vector3 ExternalForce { get; protected set; }
        public Vector3 Velocity { get; protected set; }      // velocity after modified
        public Vector3 Raw_Velocity { get; protected set; }   // velocity given by character controller

        public abstract void Move(Vector3 velocity, bool onMovingMotor = false);
        public virtual void Push(Vector3 force, bool isOnMovingMotor = false)
        {
            ExternalForce += force;
            IsOnMovingMotor = isOnMovingMotor;
        }
    }
}
