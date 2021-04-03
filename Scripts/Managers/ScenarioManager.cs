using System;
using System.Runtime.CompilerServices;
using Scenario;
using Unity.Jobs;
using UnityEngine;
using Random = System.Random;

namespace Managers
{
    public class ScenarioManager : AbstractSingleton<ScenarioManager>
    {
        [Header("Controls")]
        [SerializeField] private bool _initiateScenario;

        [Header("Settings")]
        [SerializeField] private bool _enableAvoidance;
        [SerializeField] private bool _enableAgentColliders;
        [SerializeField] private bool _enableAgentCollisions;
        private AbstractScenario _scenario;

        public bool EnableAvoidance => _enableAvoidance;
        public bool EnableAgentColliders => _enableAgentColliders;
        public bool EnableAgentCollisions => _enableAgentCollisions;

        protected override void Initialize()
        {
            EventManager.ScenarioAddedEvent += OnScenarioAdded;
            _scenario = GetComponent<AbstractScenario>();
        }

        private void OnScenarioAdded(object sender, EventArgs e)
        {
            var newScenario = sender as AbstractScenario; 

            if (_scenario != null && _scenario != newScenario)
            {
                Destroy(_scenario);
            }

            _scenario = newScenario;
        }

        void Update()
        {
            if (_initiateScenario)
            {
                if (_scenario != null)
                {
                    var timeBefore = Time.realtimeSinceStartup;
                    _scenario.Run();
                    Debug.Log($"Scenario Took {Time.realtimeSinceStartup - timeBefore} seconds");
                }

                _initiateScenario = false;
            }
        }
    }
}
