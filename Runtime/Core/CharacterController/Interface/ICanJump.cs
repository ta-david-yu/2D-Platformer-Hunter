using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface ICanJump
    {
        void PressJump();
        void HoldJump();
        void ReleaseJump();
    }
}
