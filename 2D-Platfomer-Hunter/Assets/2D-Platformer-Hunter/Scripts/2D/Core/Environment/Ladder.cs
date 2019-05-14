using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : MonoBehaviour
    {
        private BoxCollider2D m_Collider;
        public BoxCollider2D Collider
        {
            get
            {
                if (m_Collider == null)
                {
                    m_Collider = GetComponent<BoxCollider2D>();
                }
                return m_Collider;
            }
        }

        public Bounds AreaBounds { get { return Collider.bounds; } }

        [SerializeField]
        private bool m_HasRestrictedArea = false;
        public bool HasRestrictedArea { get { return m_HasRestrictedArea; } }


        [SerializeField]
        private Area2D m_RestrictedArea;
        public Area2D RestrictedArea { get { return m_RestrictedArea; } }
        public Bounds RestrictedAreaBounds { get { return RestrictedArea.Bounds; } }

        [SerializeField]
        private bool m_IsRestrictedAreaTopIgnored = true;
        public bool IsRestrictedAreaTopIgnored { get { return m_IsRestrictedAreaTopIgnored; } }

        [SerializeField]
        private float m_TopAreaHeight = 1;
        public float TopAreaHeight { get { return m_TopAreaHeight; } }

        [SerializeField]
        private float m_BottomAreaHeight = 1;
        public float BottomAreaHeight { get { return m_BottomAreaHeight; } }

        public void OnTriggerEnter2D(Collider2D col)
        {
            BasicMovementController2D character = col.GetComponent<BasicMovementController2D>();
            if (character)
            {
                //if (Collider.bounds.Contains(character.MotorCollider.bounds.center))
                character.LadderAreaEnter(AreaBounds, TopAreaHeight, BottomAreaHeight);

                if (HasRestrictedArea)
                {
                    character.SetLadderRestrictedArea(RestrictedAreaBounds, IsRestrictedAreaTopIgnored);
                }
                else
                {
                    character.ClearLadderRestrictedArea();
                }
            }
        }

        public void OnTriggerStay2D(Collider2D col)
        {
            BasicMovementController2D character = col.GetComponent<BasicMovementController2D>();
            if (character)
            {
                //if (Collider.bounds.Contains(character.MotorCollider.bounds.center))
                character.LadderAreaEnter(AreaBounds, TopAreaHeight, BottomAreaHeight);
                if (HasRestrictedArea)
                {
                    character.SetLadderRestrictedArea(RestrictedAreaBounds, IsRestrictedAreaTopIgnored);
                }
                else
                {
                    character.ClearLadderRestrictedArea();
                }
            }
        }

        public void OnTriggerExit2D(Collider2D col)
        {
            BasicMovementController2D character = col.GetComponent<BasicMovementController2D>();
            if (character)
            {
                character.LadderAreaExit();
                if (HasRestrictedArea)
                {
                    character.ClearLadderRestrictedArea();
                }
            }
        }

        private void Reset()
        {
            m_Collider = GetComponent<BoxCollider2D>();
            m_Collider.isTrigger = true;
        }
    }
}