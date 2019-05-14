using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public class PlayerInputDriver : BaseInputDriver
    {
        private void Update()
        {
            UpdateInput(Time.deltaTime);
        }

        public override void UpdateInput(float timeStep)
        {
            Horizontal = Input.GetAxisRaw("Horizontal");
            Vertical = Input.GetAxisRaw("Vertical");

            Jump = Input.GetButtonDown("Jump");
            Dash = Input.GetButtonDown("Fire3");

            HoldingJump = Input.GetButton("Jump");
            HoldingDash = Input.GetButton("Fire3");

            ReleaseJump = Input.GetButtonUp("Jump");
        }
    }
}
