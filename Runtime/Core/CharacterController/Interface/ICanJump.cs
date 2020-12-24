using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface ICanJump
    {
        public void PressJump(bool value);
        public void HoldJump(bool value);
        public void ReleaseJump(bool value);
    }
}
