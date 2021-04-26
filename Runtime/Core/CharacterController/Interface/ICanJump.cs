using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface ICanJump
    {
        void PressJump(bool value);
        void HoldJump(bool value);
        void ReleaseJump(bool value);
    }
}
