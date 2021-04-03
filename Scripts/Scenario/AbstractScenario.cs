using System;
using Managers;
using UnityEngine;

namespace Scenario
{
    public abstract class AbstractScenario : MonoBehaviour
    {
        public abstract void Run();

        void Awake()
        {
            EventManager.RaiseScenarioAddedEvent(this, EventArgs.Empty);
        }
    }
}
