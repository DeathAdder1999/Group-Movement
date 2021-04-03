using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Geometry.Primitives;
using Managers;
using Pathfinding;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace NPCs.Steering
{
    public class SteeringMovement : MonoBehaviour, IMovement
    {
        [SerializeField] private bool _isPlayer;
        private Stack<Node> _path;
        private Node _currentNode;
        private Vector2 _destination;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private CircleCollider2D _obstacleCollider;
        private Vector2? _target;
        private Group _parentGroup;
        private Vector2 _distanceToCenter;

        //TODO delete
        [SerializeField] private Vector2 _debugTarget;

        //Need to keep track if we the agent is stuck
        private Vector2 _previousPosition = new Vector2(float.MaxValue, float.MaxValue);
        //The period after which to check if the agent is stuck
        private const float StuckCheckTime = 0.1f; 
        //If after stuckCheckTime the difference between _previousDistance and Position is the less than StuckDistance it means the agent is stuck
        private const float StuckDistance = 0.01f;
        private const float AdjustDistance = 0.5f;
        private bool _stuck;

        public bool Enabled { get; set; }

        public Group ParentGroup
        {
            get => _parentGroup;
            set
            {
                _parentGroup = value;
                _parentGroup.TargetReached += OnTargetReached;
                _parentGroup.GroupLimitReached += OnGroupLimitReached;
            }
        }

        private Stack<Node> Path
        {
            get => _path;
            set => _path = value;
        }

        public Vector2 Position
        {
            get => _rb.position;
            set => _rb.position = value;
        }

        private Vector2 Velocity
        {
            get => _rb.velocity;
            set => _rb.velocity = value;
        }
        private float Mass => _rb.mass;

        void Awake()
        {
            EventManager.GameMangerReadyEvent += OnGameManagerReady;
        }

        private void OnGameManagerReady(object sender, EventArgs e)
        {
            Initialize();
            StartCoroutine(CheckIfStuck());
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!Enabled)
            {
                return;
            }

            CheckColliders();

            if (_stuck)
            {
                if (_target.HasValue)
                {
                    Position -= (_target.Value - Position).normalized * AdjustDistance * Time.deltaTime;
                }
            }

            if (_target != null)
            {
                _debugTarget = _target.Value;
                Seek(_target.Value);

                if (ScenarioManager.Instance.EnableAvoidance)
                {
                    Avoid();
                }

                var brakeCoefficient = 1.0f;
                Velocity *= Time.deltaTime * brakeCoefficient;
            }
            else
            {
                Velocity = Vector2.zero;
            }
        }

        private void CheckColliders()
        {
            var colliderSetting = ScenarioManager.Instance.EnableAgentColliders;

            if (ScenarioManager.Instance.EnableAgentColliders != _collider.enabled)
            {
                _collider.enabled = colliderSetting;
                _obstacleCollider.enabled = colliderSetting;
            }
        }

        private IEnumerator CheckIfStuck()
        {
            while (true)
            {
                yield return new WaitForSeconds(StuckCheckTime);

                //If not moving
                if (_target == null || Utils.IsCloserThan(Position, _target.Value + _distanceToCenter, 0.01f))
                {
                    continue;
                }

                //var rayCastResult = Physics2D.Raycast(Position, Velocity, StuckDistance).collider;

                if (Utils.IsCloserThan(Position, _previousPosition, StuckDistance) /*|| Mathf.Approximately(Velocity.sqrMagnitude, 0)*/)
                {
                    _stuck = true;
                }
                else
                {
                    _stuck = false;
                }

                _previousPosition = Position;
            }
        }

        private void FollowPath()
        {
            if (_path != null && _path.Any())
            {
                _target = _target ?? _path.Peek().Center;

                if (Utils.IsCloserThan(Position, _target.Value, SteeringProperties.PathFollowingRadius))
                {
                    _target = _path.Pop().Center;
                }
            }
            else
            {
                _target = null;
                Path = null;
            }

            if (_target != null)
            {
                Seek(_target.Value);
            }
        }

        private void Avoid()
        {
            var velocityNormalized = Velocity.normalized;
            var ahead = Position + velocityNormalized * SteeringProperties.MaxSeeAhead;
            var closestObstacle = GetClosestObstacle();
            var closestFriendlyObstacle = GetClosestFriendlyObstacle();
            var friendlyObstacleAvoidanceVector = new Vector2(Velocity.y, -Velocity.x);
            var obstacleAvoidanceVector = GetObstacleAvoidanceVector(closestObstacle);
            var avoidance = new Vector2();  

            if (closestFriendlyObstacle != null && closestObstacle != null)
            {
                avoidance = ahead + obstacleAvoidanceVector + friendlyObstacleAvoidanceVector;
            }
            else if (closestFriendlyObstacle != null)
            {
                avoidance = ahead + friendlyObstacleAvoidanceVector;
            }
            else if (closestObstacle != null)
            {
                avoidance = ahead - obstacleAvoidanceVector;
            }

            avoidance.Normalize();
            var avoidanceForce = _stuck ? SteeringProperties.MaxAvoidForce : SteeringProperties.MaxAvoidForce * 1;
            avoidance *= avoidanceForce;

            ApplyForce(avoidance);
        }

        private Vector2 GetObstacleAvoidanceVector(GameObject obstacle)
        {
            if (obstacle == null)
            {
                return Vector2.zero;
            }

            var line = obstacle.transform.parent.gameObject.GetComponent<Line>();
            var avoidanceVector = Vector2.zero;//-obstacle.transform.position;

            if (line != null)
            {
                var startToEnd = line.EndPoint - line.StartPoint;
                var endToStart = line.StartPoint - line.EndPoint;

                avoidanceVector = (Velocity + startToEnd).sqrMagnitude > (Velocity + endToStart).sqrMagnitude ? startToEnd : endToStart;
            }

            return Vector2.Perpendicular(avoidanceVector.normalized * 100f);
        }

        private GameObject GetClosestObstacle()
        {
            var hits = Physics2D.RaycastAll(Position, Velocity, SteeringProperties.MaxSeeAhead);
            GameObject obstacle = null;

            foreach (var hit in hits)
            {
                if (hit.collider.tag == "Obstacle")
                {
                    obstacle = hit.collider.gameObject;
                }
                else
                {
                    var children = hit.collider.transform.GetChildren();

                    foreach (var child in children)
                    {
                        if (child.gameObject.tag == "Obstacle")
                        {
                            obstacle = hit.collider.gameObject;
                        }
                    }
                }
            }

            return obstacle;
        }

        private GameObject GetClosestFriendlyObstacle()
        {
            var hits = Physics2D.RaycastAll(Position, Velocity, SteeringProperties.MaxFriendlySeeAhead);
            GameObject obstacle = null;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.transform.parent == transform)
                {
                    continue;
                }

                if (hit.collider.tag == "FriendlyObstacle")
                {
                    obstacle = hit.collider.gameObject;
                }
                else if(GroupMovementManager.Instance.ContainsGroupTag(hit.collider.tag))
                {
                    var children = hit.collider.transform.GetChildren();

                    foreach (var child in children)
                    {
                        if (child.gameObject.tag == "FriendlyObstacle")
                        {
                            obstacle = hit.collider.gameObject;
                        }
                    }
                }
            }

            return obstacle;
        }

        private void Seek(Vector2 target)
        {
            var desiredVelocity = (target - Position).normalized * SteeringProperties.MaxSpeed;
            var steering = desiredVelocity - Velocity;

            ApplyForce(steering);
        }

        private void ApplyForce(Vector2 force)
        {
            var steering = Utils.Clamp(force, SteeringProperties.MinForce, SteeringProperties.MaxForce);
            steering = steering / Mass;

            Velocity = Utils.Clamp(Velocity + steering, SteeringProperties.MinSpeed, SteeringProperties.MaxSpeed);
        }

        private void Initialize()
        {
            _collider = GetComponent<BoxCollider2D>();
            _obstacleCollider = transform.GetChildren()[0].gameObject.GetComponent<CircleCollider2D>();
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.mass = 1;
            _rb.gravityScale = 0;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void SetDestination(Vector2Int destination)
        {
            Debug.Log("Nothing");
        }

        public void SetDestination(Vector2 destination)
        {
            _target = destination + _distanceToCenter;
        }

        public void SetPath(Stack<Node> path)
        {
            var pathCopy = new Stack<Node>();

            foreach (var node in path)
            {
                pathCopy.Push(node);
            }

            Path = pathCopy;
        }

        private void OnTargetReached(object sender, EventArgs e)
        {
            _target = null;
            Velocity = Vector2.zero;
            Position = ParentGroup.Center + _distanceToCenter;
        }

        private void OnGroupLimitReached(object sender, EventArgs e)
        {
            _distanceToCenter = ParentGroup.Center - Position;
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            //TODO delete

            if (col.gameObject.tag == tag)
            {
                Physics2D.IgnoreCollision(col.collider, _collider);
            }

            if (!ScenarioManager.Instance.EnableAgentCollisions)
            {
                Physics2D.IgnoreCollision(col.collider, _collider);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            if (_rb != null)
            {
                //Gizmos.DrawRay(Position, Velocity);
            }

            Gizmos.color = Color.red;

            if (_rb != null)
            {
                //Gizmos.DrawRay(Position, Velocity.normalized * SteeringProperties.MaxSeeAhead);
            }
        }
    }
}
