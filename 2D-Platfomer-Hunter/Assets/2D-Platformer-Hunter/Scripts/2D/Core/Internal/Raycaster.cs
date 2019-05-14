using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    // Hide in component menu
    [AddComponentMenu("")]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Raycaster : MonoBehaviour
    {
        [System.Serializable]
        public struct RaycastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }

        public const float c_SkinWidth = .015f;

        // Reference
        public Collider2D Collider { get; protected set; }
        private RaycastOrigins m_Origins;
        public RaycastOrigins Origins { get { return m_Origins; } }

        [Header("Settings")]
        [SerializeField]
        private LayerMask m_CollisionLayer;
        public LayerMask CollisionLayer { get { return m_CollisionLayer; } }
        [SerializeField]
        private int m_HorizontalRayCount = 4;
        public int HorizontalRayCount { get { return m_HorizontalRayCount; } }
        [SerializeField]
        private int m_VerticalRayCount = 4;
        public int VerticalRayCount { get { return m_VerticalRayCount; } }

        private float m_HorizontalRaySpacing;
        public float HorizontalRaySpacing { get { return m_HorizontalRaySpacing; } }
        private float m_VerticalRaySpacing;
        public float VerticalRaySpacing { get { return m_VerticalRaySpacing; } }

        private void Awake()
        {
            Collider = GetComponent<Collider2D>();
        }

        public void Init()
        {
            CalculateRaySpacing();
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = Collider.bounds;
            bounds.Expand(c_SkinWidth * -2);

            m_Origins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            m_Origins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            m_Origins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            m_Origins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            Bounds bounds = Collider.bounds;
            bounds.Expand(c_SkinWidth * -2);

            // min should be 2
            if (m_HorizontalRayCount < 2)
                m_HorizontalRayCount = 2;
            // min should be 2
            if (m_VerticalRayCount < 2)
                m_VerticalRayCount = 2;

            m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
            m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
        }
    }
}
