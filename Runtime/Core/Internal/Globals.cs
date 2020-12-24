using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    // to indicate which zone of a ladder the motor is in
    public enum LadderZone
    {
        Top,
        Middle,
        Bottom
    }

    public enum MotorState
    {
        OnGround,           // on ground
        Jumping,            // not on ground, velocity.y > 0
        Falling,            // not on ground, velocity.y < 0
        WallSliding,        // on wall
        Dashing,            // dashing
        OnLadder,           // freedom state, can move vertically and horizontally freely

        Frozen,             // forzen, TODO
        OnLedge,            // ledge grabbing, TODO

        CustomAction,       // custom action performed by action module
    }

    public struct MotorCollision2D
    {
        public enum CollisionSurface
        {
            None = 0x0,
            Ground = 0x1,
            Left = 0x2,
            Right = 0x4,
            Ceiling = 0x8
        }

        public CollisionSurface Surface;

        public bool IsSurface(CollisionSurface surface)
        {
            return (surface & Surface) != CollisionSurface.None;
        }
    }
}