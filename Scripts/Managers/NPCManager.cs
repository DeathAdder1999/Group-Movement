using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Polygons;
using NPCs;
using UnityEngine;

namespace Managers
{
    public class NPCManager : AbstractSingleton<NPCManager>
    {
        private List<NPC> _npcs = new List<NPC>();
        private List<Player> _players = new List<Player>();
        private int _totalPlayerCount;


        public void AddNpc(NPC npc)
        {
            _npcs.Add(npc);
        }

        public void RemoveNpc(NPC npc)
        {
            _npcs.Remove(npc);
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player);

            if (_players.Count == _totalPlayerCount)
            {
                EventManager.RaisePlayersReadyEvent(this, EventArgs.Empty);
            }
        }

        public void RemovePlayer(Player player)
        {
            _players.Remove(player);
        }

        public List<Polygon> GetFovPolygons()
        {
            var fovPolygons = new List<Polygon>();

            foreach (var npc in _npcs)
            {
               Debug.Log($"Npc polygon");
               var polygon = npc.FovPolygon;
               fovPolygons.Add(polygon);
            }

            return fovPolygons;
        }

        protected override void Initialize()
        {
            EventManager.NpcDestroyedEvent += OnNpcDestroyedEvent;
            _totalPlayerCount = FindObjectsOfType<Player>().Length;
        }

        private void OnNpcDestroyedEvent(object sender, EventArgs e)
        {
            var npc = sender as NPC;
            RemoveNpc(npc);
        }
    }
}
