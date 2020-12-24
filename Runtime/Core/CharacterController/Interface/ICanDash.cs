using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public interface ICanDash
    {
        public void PressDash(bool value);
        public void HoldDash(bool value);
    }
}
