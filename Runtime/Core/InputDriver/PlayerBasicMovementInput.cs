using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    /// <summary>
    /// The player input driver for controller BasicMovementController2D
    /// </summary>
    public class PlayerBasicMovementInput : MonoBehaviour
    {
        [SerializeField]
        private BasicMovementController2D m_Controller;

        // Update is called once per frame
        void Update()
        {
            //m_Controller.ClearInputBuffer(Time.deltaTime);

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            var jump = Input.GetButtonDown("Jump");
            var dash = Input.GetButtonDown("Fire3");

            var holdingJump = Input.GetButton("Jump");
            var holdingDash = Input.GetButton("Fire3");

            var releaseJump = Input.GetButtonUp("Jump");

            m_Controller.InputMovement(new Vector2(horizontal, vertical));

            m_Controller.PressJump(jump);

            m_Controller.PressDash(dash);

            m_Controller.HoldJump(holdingJump);

            m_Controller.HoldDash(holdingDash);

            m_Controller.ReleaseJump(releaseJump);
        }
    }
}