﻿using System.Collections.Generic;
using Geometry.Polygons;
using UnityEngine;

namespace Main
{
    public static class Properties
    {
        // General Settings
        public static string LogFilesPath = "c:/LogFiles/";
        public static string MapsPath = "Assets/MapData/";
        public static string MapFileDotExt = ".csv";
        public static char MapFileSeparator = ',';
        
        // Winding order for outer polygons; inner polygon is opposite.
        public static WindingOrder OuterPolygonWinding = WindingOrder.CounterClockwise;
        public static WindingOrder InnerPolygonWinding = WindingOrder.Clockwise;

        // Polygon Smoothing parameters
        public static float MinAngle = 10f;
        public static float MaxAngle = 170f;

        // The walkalbe area offset from the actual map
        public static float InterPolygonOffset = 0.2f;

        /*
        // The map scale 
        public static float GetDefaultMapScale(Map map)
        {
            switch (map)
            {
                case Map.MgsDock:
                    return 2f;
    
                case Map.AlienIsolation:
                    return 3f;
    
                case Map.Arkham:
                    return 2f;
    
                default:
                    return 1f;
            }
        }*/

        // The number of episodes to record
        public static int EpisodesCount = 6;

        // Staleness Properties
        // Staleness range
        // Colors for view point state
        public const byte StalenessHigh = 255;
        public const byte StalenessLow = 0;

        public static Color32 GetStalenessColor(float staleness)
        {
            float cappedStaleness = Mathf.Min(staleness, StalenessHigh);
            byte colorLevel = (byte) (StalenessHigh - cappedStaleness);
            return new Color32(colorLevel, colorLevel, colorLevel, 120);
        }

        // Staleness rate per second
        public const float StalenessRate = 16f;

        // TimeStep required to cover one distance unit in seconds
        public static float TimeRequiredToCoverOneUnit = 3f;

        // Hiding Spots
        // Number of static hiding spots
        public static readonly int HidingSpotsCount = 50;

        // NPC Properties
        // NPC Movement
        // How accelerated the game should
        private const float SimulationSpeedMultiplier = 1f;

        // How fast the NPC moves
        public const float NpcSpeed = SimulationSpeedMultiplier * 2f;
        public const float NpcRotationSpeed = SimulationSpeedMultiplier * 100f;

        // Field of View Properties
        public const float ViewRadius = 2f;
        public const float ViewAngle = 360f;


        // Grid parameters representation
        static readonly int GridMultiplier = 1;
        public static readonly int GridDefaultSizeX = 16 * GridMultiplier;
        public static readonly int GridDefaultSizeY = 10 * GridMultiplier;
        public static readonly float NodeRadius = 0.1f;
        public static readonly int GridRows = 15;
        public static readonly int GridColumns = 20;

        //Polygon Lines
        public static float LineStartWidth = 0.06f;
        public static float LineEndWidth = 0.03f;
        public static float LineColliderOffset = 0.05f;

        //NPC spawn point
        public static readonly Vector2Int Spawn = new Vector2Int(52, 24);

        public static readonly int GroupSize = 6;
    }
}
