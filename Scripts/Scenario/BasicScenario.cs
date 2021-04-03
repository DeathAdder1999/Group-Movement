using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Scenario
{
    public class BasicScenario : AbstractScenario
    {
        [SerializeField] private Vector2 _destination;
        [SerializeField] private string _groupTag;

        public override void Run()
        {
            GroupMovementManager.Instance.SetDestination(_destination, _groupTag);
        }
    }
}
