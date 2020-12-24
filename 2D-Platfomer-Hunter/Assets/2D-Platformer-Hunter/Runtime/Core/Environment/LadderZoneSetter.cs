using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DYP
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LadderZoneSetter : MonoBehaviour
    {
        public LadderZone Zone;

        public void OnTriggerEnter2D(Collider2D o)
        {
            BasicMovementController2D character = o.GetComponent<BasicMovementController2D>();
            if (character)
            {
                character.SetLadderZone(Zone);
            }
        }
        public void OnTriggerStay2D(Collider2D o)
        {
            BasicMovementController2D character = o.GetComponent<BasicMovementController2D>();
            if (character)
            {
                character.SetLadderZone(Zone);
            }
        }

        private void Reset()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }
    }
}
