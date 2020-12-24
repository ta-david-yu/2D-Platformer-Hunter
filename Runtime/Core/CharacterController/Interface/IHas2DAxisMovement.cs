using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface IHas2DAxisMovement
    {
        public void InputMovement(Vector2 axis);
        public float MovementSpeed { get; }
    }
}