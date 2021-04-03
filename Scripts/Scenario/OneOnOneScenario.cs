using UnityEngine;
using Managers;

namespace Scenario
{
    public class OneOnOneScenario : AbstractScenario
    {
        public override void Run()
        {
            Debug.Log("RUN");
            //Assumption is that we have at least 2 players
            var tag1 = GroupMovementManager.Instance.GetGroupTag(0);
            var tag2 = GroupMovementManager.Instance.GetGroupTag(1);
            var group1 = GroupMovementManager.Instance.GetGroup(tag1);

            //Group2 to position of Group1
            GroupMovementManager.Instance.SetDestination(group1.Center, tag2);
        }
    }
}
