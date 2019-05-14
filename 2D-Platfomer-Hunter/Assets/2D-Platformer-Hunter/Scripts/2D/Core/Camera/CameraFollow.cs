using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public class CameraFollow : MonoBehaviour
    {
        [System.Serializable]
        struct FocusArea
        {
            public Vector2 Center { get; private set; }
            public Vector2 Velocity { get; private set; }
            public float Left { get; private set; }
            public float Right { get; private set; }
            public float Top { get; private set; }
            public float Bottom { get; private set; }

            private float m_SmoothVelocityY;

            public FocusArea(Bounds targetBounds, Vector2 size)
            {
                Left = targetBounds.center.x - size.x / 2;
                Right = targetBounds.center.x + size.x / 2;

                Bottom = targetBounds.min.y;
                Top = targetBounds.min.y + size.y;

                Velocity = Vector2.zero;
                Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);

                m_SmoothVelocityY = 0;
            }

            public void Update(Bounds targetBounds)
            {
                float shiftX = 0;

                /*
                 * Horizontal Shift
                 */
                // target went too left
                if (targetBounds.min.x < Left)
                {
                    shiftX = targetBounds.min.x - Left;
                }
                // target went too right
                else if (targetBounds.max.x > Right)
                {
                    shiftX = targetBounds.max.x - Right;
                }

                // LERP EFFECT !!
                //shiftX = Mathf.Lerp(Center.x, targetBounds.center.x, Time.deltaTime * 4.0f) - Center.x;

                Left += shiftX;
                Right += shiftX;

                float shiftY = 0;

                /*
                 * Vertical Shift
                 */

                shiftY = Mathf.SmoothDamp(Center.y, targetBounds.center.y, ref m_SmoothVelocityY, 0.2f) - Center.y;

                //shiftY = Mathf.Lerp(Center.y, targetBounds.center.y, Time.deltaTime * 4.0f) - Center.y;

                Top += shiftY;
                Bottom += shiftY;

                Velocity = new Vector2(shiftX, shiftY);
                Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
            }
        }

        [Header("Reference")]

        [SerializeField]
        private Raycaster m_MainTarget;
        public Raycaster MainTarget { get { return m_MainTarget; } private set { m_MainTarget = value; } }

        [Header("Settings")]
        [SerializeField]
        private Vector2 m_FocusAreaSize = new Vector2(3, 1);

        [SerializeField]
        private float m_VerticalOffset = 1.0f;

        [SerializeField]
        [Tooltip("The smooth speed when the target is out of bound, changing offset.")]
        private float m_HorizontalSmoothSpeed = 1.0f;

        [SerializeField]
        [Tooltip("The smooth speed when the target is out of bound.")]
        private float m_HorizontalMaxOffset = 4.0f;

        private FocusArea m_FocusArea;

        private float m_CurrHorizontalOffset = 0.0f;            // -m_HorizontalMaxOffset ~ m_HorizontalMaxOffset
        private float m_TargetHorizontalOffset = 0.0f;

        private float m_TargetPositionX = 0;
        private float m_TargetVelocityX = 0;

        private void Start()
        {
            m_FocusArea = new FocusArea(MainTarget.Collider.bounds, m_FocusAreaSize);
            m_TargetPositionX = MainTarget.transform.position.x;
        }

        private void LateUpdate()
        {
            m_FocusArea.Update(MainTarget.Collider.bounds);

            m_TargetVelocityX = MainTarget.transform.position.x - m_TargetPositionX;

            if (m_FocusArea.Velocity.x != 0.0)
            {
                m_TargetHorizontalOffset += m_FocusArea.Velocity.x;
                m_TargetHorizontalOffset =
                    Mathf.Clamp(m_TargetHorizontalOffset, -m_HorizontalMaxOffset, m_HorizontalMaxOffset);
            }

            m_CurrHorizontalOffset =
                Mathf.Lerp(m_CurrHorizontalOffset, m_TargetHorizontalOffset, Time.deltaTime * m_HorizontalSmoothSpeed);

            Vector2 focusPosition =
                m_FocusArea.Center + Vector2.up * m_VerticalOffset + Vector2.right * m_CurrHorizontalOffset;

            transform.position = (Vector3)focusPosition + Vector3.forward * -10.0f;

            m_TargetPositionX = MainTarget.transform.position.x;
        }

        public void SetMainTarget(Raycaster _target)
        {
            MainTarget = _target;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(m_FocusArea.Center, m_FocusAreaSize);
        }
    }
}
