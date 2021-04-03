using System;
using Pathfinding;
using UnityEditor.Animations;
using UnityEngine;

namespace Managers
{
    public static class EventManager
    {
        public static EventHandler MapGeneratedEvent;
        public static EventHandler GridGeneratedEvent;
        public static EventHandler GameMangerReadyEvent;
        public static EventHandler<Vector2Int> NpcDestinationSetEvent;
        public static EventHandler NpcDestroyedEvent;
        public static EventHandler NpcCreatedEvent;
        public static EventHandler ShadowVolumeChangedEvent;
        public static EventHandler MapPassabilityChangedEvent;
        public static EventHandler SnapshotMergedEvent;
        public static EventHandler PlayersReadyEvent;
        public static EventHandler ScenarioAddedEvent;

        public static void RaiseMapGeneratedEvent(object sender, EventArgs args)
        {
            MapGeneratedEvent?.Invoke(sender, args);
        }

        public static void RaiseGridGeneratedEvent(object sender, EventArgs args)
        {
            GridGeneratedEvent?.Invoke(sender, args);
        }

        public static void RaiseNpcDestinationSetEvent(object sender, Vector2Int destination)
        {
            NpcDestinationSetEvent?.Invoke(sender, destination);
        }

        public static void RaiseNpcDestroyedEvent(object sender, EventArgs e)
        {
            NpcDestroyedEvent?.Invoke(sender, e);
        }

        public static void RaiseNpcCreatedEvent(object sender, EventArgs e)
        {
            NpcCreatedEvent?.Invoke(sender, e);
        }

        public static void RaiseShadowVolumeChangedEvent(object sender, EventArgs e)
        {
            ShadowVolumeChangedEvent?.Invoke(sender, e);
        }

        public static void RaiseGameManagerReadyEvent(object sender, EventArgs e)
        {
            GameMangerReadyEvent?.Invoke(sender, e);
        }

        public static void RaiseMapPassabilityChangedEvent(object sender, EventArgs e)
        {
            MapPassabilityChangedEvent?.Invoke(sender, e);
        }

        public static void RaiseSnapshotMergedEvent(object sender, EventArgs e)
        {
            SnapshotMergedEvent?.Invoke(sender, e);
        }

        public static void RaisePlayersReadyEvent(object sender, EventArgs e)
        {
            PlayersReadyEvent?.Invoke(sender, e);
        }

        public static void RaiseScenarioAddedEvent(object sender, EventArgs e)
        {
            ScenarioAddedEvent?.Invoke(sender, e);
        }
    }
}
