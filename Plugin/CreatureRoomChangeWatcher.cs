using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using ThunderRoad;
using Chabuk.ManikinMono;

namespace AcidSpell
{
    class CreatureRoomChangeWatcher : MonoBehaviour
    {
        Creature creature = null;
        Room prevRoom = null;

        public delegate void RoomChanged(Creature c, Room r);
        public event RoomChanged onRoomChanged;

        public void Awake()
        {
            creature = gameObject.GetComponentInChildren<Creature>();
        }

        public void LateUpdate()
        {
            if(creature.currentRoom != prevRoom)
            {
                if(onRoomChanged != null)
                {
                    onRoomChanged(creature, creature.currentRoom);
                }
            }
            prevRoom = creature.currentRoom;
        }
    }
}
