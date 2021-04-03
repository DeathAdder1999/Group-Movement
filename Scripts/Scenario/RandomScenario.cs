using Managers;
using UnityEngine;
using Random = System.Random;

namespace Scenario
{
    public class RandomScenario : AbstractScenario
    {
        private static Random _random = new Random();

        public override void Run()
        {
            var numGroups = GroupMovementManager.Instance.NumberOfGroups;
            var xLimits = GameManager.Instance.GetGridXDimensions();
            var yLimits = GameManager.Instance.GetGridYDimensions();
            var numIterations = _random.Next(1, numGroups);

            for (var i = 1; i < numIterations; i++)
            {
                var index = _random.Next(1, numGroups);
                var groupName = GroupMovementManager.Instance.GetGroupTag(index);
                var randomDestination = Utils.GetRandomVector(xLimits, yLimits);

                GroupMovementManager.Instance.SetDestination(randomDestination, groupName);
            }
        }
    }
}
