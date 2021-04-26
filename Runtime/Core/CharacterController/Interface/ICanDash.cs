using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface ICanDash
    {
        void PressDash(bool value);
        void HoldDash(bool value);
    }
}
