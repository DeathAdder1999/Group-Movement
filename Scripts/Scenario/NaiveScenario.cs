using Managers;
using UnityEngine;

namespace Scenario
{
    public class NaiveScenario : AbstractScenario
    {
        [SerializeField] private Vector2 _destination;
        [SerializeField] private string _groupTag;

        public override void Run()
        {
            GroupMovementManager.Instance.SetDestinationGrid(_destination, _groupTag);
        }
    }
}
