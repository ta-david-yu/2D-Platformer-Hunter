using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface IHas2DAxisMovement
    {
        void InputMovement(Vector2 axis);
        float MovementSpeed { get; }
    }
}