using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    public class Area2D : MonoBehaviour
    {
        public Color _debugColor = new Color(1.0f, 0.92f, 0.016f, 0.1f);

        [SerializeField]
        [HideInInspector]
        private Bounds m_Bounds = new Bounds();
        public Bounds Bounds { get { return m_Bounds; } set { m_Bounds = value; } }

        public Vector2 Center
        {
            get
            {
                return transform.position + (Vector3)Offset;
            }
        }

        [SerializeField]
        [HideInInspector]
        private Vector2 m_Offset = Vector2.zero;
        public Vector2 Offset
        {
            get { return m_Offset; }
            set
            {
                m_Offset = value;
                m_Bounds.center = transform.position + (Vector3)m_Offset;
            }
        }

        [SerializeField]
        [HideInInspector]
        private Vector2 m_Extents = Vector2.zero;
        public Vector2 Extents
        {
            get { return m_Extents; }
            set
            {
                m_Extents = value;
                m_Bounds.extents = m_Extents;
            }
        }

        public Vector2 Max { get { return Center + Extents; } }
        public Vector2 Min { get { return Center - Extents; } }

        private void Reset()
        {
            var parent = transform.parent;
            var col = (parent) ? parent.GetComponent<Collider2D>() : null;

            if (col)
            {
                Offset = col.offset;
                Extents = col.bounds.extents;
            }
            else
            {
                Offset = Vector2.zero;
                Extents = Vector2.one;
            }
        }
    }
}
