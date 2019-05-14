using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public abstract class BaseInputDriver : MonoBehaviour
    {
        public float Horizontal { get; protected set; }
        public float Vertical { get; protected set; }
        public bool Jump { get; protected set; }
        public bool HoldingJump { get; protected set; }
        public bool ReleaseJump { get; protected set; }

        public bool Dash { get; protected set; }
        public bool HoldingDash { get; protected set; }
        public bool ReleaseDash { get; protected set; }

        public abstract void UpdateInput(float timeStep);
    }
}
