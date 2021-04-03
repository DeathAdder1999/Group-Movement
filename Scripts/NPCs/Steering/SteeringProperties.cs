using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCs.Steering
{
    public static class SteeringProperties
    {
        public static readonly float MaxSpeed = 30.0f;
        public static readonly float MinSpeed = 15.0f;
        public static readonly float MaxForce = 30.0f;
        public static readonly float MinForce = 4.0f;
        public static readonly float PathFollowingRadius = 1f;
        public static readonly float MinimumStopDistance = 0.05f;
        public static readonly float DeltaTimeCoefficitent = 10.0f;
        public static readonly float LineObstacleWidth = 1.0f;
        public static readonly float MaxSeeAhead = 1f;
        public static readonly float MaxFriendlySeeAhead = 0.3f;
        public static readonly float MaxAvoidForce = 10.0f;
    }
}
