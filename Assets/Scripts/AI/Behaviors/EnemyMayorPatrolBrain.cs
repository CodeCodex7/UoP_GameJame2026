using AI.Goap.UnitAI.Goal;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Behaviors
{
    [RequireComponent(typeof(UnitAIBrain))]
    public class EnemyMayorPatrolBrain : MonoBehaviour
    {
        [SerializeField] private bool patrolClockwise = true;
        [SerializeField] private GameObject groundPlane;
        [SerializeField] private bool useGroundPlaneBounds = true;
        [SerializeField] private Vector2 xBounds = new Vector2(-45f, 45f);
        [SerializeField] private Vector2 zBounds = new Vector2(-45f, 45f);
        [SerializeField] private float edgeInset = 4f;
        [SerializeField] private float patrolHopDistance = 8f;
        [SerializeField] private float waypointReachedDistance = 3f;
        [SerializeField] private Vector2 stopDurationRange = new Vector2(60f, 120f);
        [SerializeField] private float orderRefreshInterval = 0.75f;
        [SerializeField] private float patrolResumeDelay = 2f;
        [SerializeField] private float navMeshSampleRadius = 8f;

        private readonly Vector3[] waypoints = new Vector3[4];
        private readonly Vector3[] routePoints = new Vector3[4];
        private UnitAIBrain mayor;
        private int routeSegmentIndex;
        private float routeSegmentProgress;
        private float nextThinkTime;
        private float resumePatrolTime;
        private float waitUntilTime;
        private Vector3 currentPatrolTarget;
        private Vector3 lastIssuedTarget;
        private bool hasPatrolTarget;
        private bool hasIssuedTarget;

        private void Awake()
        {
            mayor = GetComponent<UnitAIBrain>();
            RebuildWaypoints();
            SetRouteToClosestPoint();
        }

        private void OnValidate()
        {
            edgeInset = Mathf.Max(0f, edgeInset);
            patrolHopDistance = Mathf.Max(0.5f, patrolHopDistance);
            waypointReachedDistance = Mathf.Max(0.1f, waypointReachedDistance);
            orderRefreshInterval = Mathf.Max(0.1f, orderRefreshInterval);
            patrolResumeDelay = Mathf.Max(0f, patrolResumeDelay);
            navMeshSampleRadius = Mathf.Max(0.1f, navMeshSampleRadius);

            if (xBounds.x > xBounds.y)
            {
                xBounds = new Vector2(xBounds.y, xBounds.x);
            }

            if (zBounds.x > zBounds.y)
            {
                zBounds = new Vector2(zBounds.y, zBounds.x);
            }

            if (stopDurationRange.x < 0f)
            {
                stopDurationRange.x = 0f;
            }

            if (stopDurationRange.y < 0f)
            {
                stopDurationRange.y = 0f;
            }

            if (stopDurationRange.y < stopDurationRange.x)
            {
                stopDurationRange = new Vector2(stopDurationRange.y, stopDurationRange.x);
            }

            RebuildWaypoints();
        }

        public void SetGroundPlane(GameObject newGroundPlane)
        {
            groundPlane = newGroundPlane;
            RebuildWaypoints();
            SetRouteToClosestPoint();
            hasPatrolTarget = false;
            hasIssuedTarget = false;
        }

        private void Update()
        {
            if (Time.time < nextThinkTime || mayor == null || !mayor.IsAlive)
            {
                return;
            }

            nextThinkTime = Time.time + orderRefreshInterval;

            if (HasNearbyThreat())
            {
                resumePatrolTime = Time.time + patrolResumeDelay;
                mayor.provider.RequestGoal<FleeGoal>(true);
                hasPatrolTarget = false;
                hasIssuedTarget = false;
                return;
            }

            if (Time.time < resumePatrolTime || Time.time < waitUntilTime)
            {
                return;
            }

            PatrolOuterEdge();
        }

        private void PatrolOuterEdge()
        {
            var flatPosition = Flatten(transform.position);

            if (!hasPatrolTarget)
            {
                currentPatrolTarget = GetNextPatrolTarget();
                hasPatrolTarget = true;
                hasIssuedTarget = false;
            }

            if ((flatPosition - Flatten(currentPatrolTarget)).sqrMagnitude <= waypointReachedDistance * waypointReachedDistance)
            {
                hasPatrolTarget = false;
                hasIssuedTarget = false;
                waitUntilTime = Time.time + Random.Range(stopDurationRange.x, stopDurationRange.y);
                mayor.provider.RequestGoal<IdleGoal>(true);
                return;
            }

            if (!TryGetNavMeshPosition(currentPatrolTarget, out var navMeshTarget))
            {
                return;
            }

            if (hasIssuedTarget && (lastIssuedTarget - navMeshTarget).sqrMagnitude < 1f)
            {
                return;
            }

            lastIssuedTarget = navMeshTarget;
            hasIssuedTarget = true;
            mayor.MoveOrder(navMeshTarget);
        }

        private bool HasNearbyThreat()
        {
            return Services.TryResolve<GameDataStore>(out var gameDataStore) &&
                   gameDataStore.TryGetClosestRivalUnit(mayor, mayor.FleeSearchRadius, out _);
        }

        private void RebuildWaypoints()
        {
            UpdateBoundsFromGroundPlane();

            var minX = xBounds.x + edgeInset;
            var maxX = xBounds.y - edgeInset;
            var minZ = zBounds.x + edgeInset;
            var maxZ = zBounds.y - edgeInset;

            if (minX > maxX)
            {
                var centreX = (xBounds.x + xBounds.y) * 0.5f;
                minX = centreX;
                maxX = centreX;
            }

            if (minZ > maxZ)
            {
                var centreZ = (zBounds.x + zBounds.y) * 0.5f;
                minZ = centreZ;
                maxZ = centreZ;
            }

            waypoints[0] = new Vector3(minX, transform.position.y, minZ);
            waypoints[1] = new Vector3(maxX, transform.position.y, minZ);
            waypoints[2] = new Vector3(maxX, transform.position.y, maxZ);
            waypoints[3] = new Vector3(minX, transform.position.y, maxZ);

            for (var i = 0; i < waypoints.Length; i++)
            {
                routePoints[i] = patrolClockwise ? waypoints[i] : waypoints[waypoints.Length - 1 - i];
            }
        }

        private void UpdateBoundsFromGroundPlane()
        {
            if (!useGroundPlaneBounds || groundPlane == null || !TryGetGroundBounds(out var groundBounds))
            {
                return;
            }

            xBounds = new Vector2(groundBounds.min.x, groundBounds.max.x);
            zBounds = new Vector2(groundBounds.min.z, groundBounds.max.z);
        }

        private bool TryGetGroundBounds(out Bounds bounds)
        {
            if (groundPlane.TryGetComponent<Collider>(out var groundCollider))
            {
                bounds = groundCollider.bounds;
                return true;
            }

            if (groundPlane.TryGetComponent<Renderer>(out var groundRenderer))
            {
                bounds = groundRenderer.bounds;
                return true;
            }

            if (groundPlane.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
            {
                bounds = TransformLocalBoundsToWorld(meshFilter.sharedMesh.bounds, groundPlane.transform);
                return true;
            }

            bounds = default;
            return false;
        }

        private static Bounds TransformLocalBoundsToWorld(Bounds localBounds, Transform boundsTransform)
        {
            var worldBounds = new Bounds(boundsTransform.TransformPoint(localBounds.center), Vector3.zero);
            var extents = localBounds.extents;

            for (var x = -1; x <= 1; x += 2)
            {
                for (var y = -1; y <= 1; y += 2)
                {
                    for (var z = -1; z <= 1; z += 2)
                    {
                        var corner = localBounds.center + Vector3.Scale(extents, new Vector3(x, y, z));
                        worldBounds.Encapsulate(boundsTransform.TransformPoint(corner));
                    }
                }
            }

            return worldBounds;
        }

        private void SetRouteToClosestPoint()
        {
            routeSegmentIndex = 0;
            routeSegmentProgress = 0f;
            var closestDistanceSqr = float.MaxValue;
            var flatPosition = Flatten(transform.position);

            for (var i = 0; i < routePoints.Length; i++)
            {
                var start = Flatten(routePoints[i]);
                var end = Flatten(routePoints[(i + 1) % routePoints.Length]);
                var segment = end - start;
                var segmentLengthSqr = segment.sqrMagnitude;

                if (segmentLengthSqr <= 0.01f)
                {
                    continue;
                }

                var progressPercent = Mathf.Clamp01(Vector3.Dot(flatPosition - start, segment) / segmentLengthSqr);
                var closestPoint = start + segment * progressPercent;
                var distanceSqr = (flatPosition - closestPoint).sqrMagnitude;

                if (distanceSqr >= closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                routeSegmentIndex = i;
                routeSegmentProgress = Mathf.Sqrt(segmentLengthSqr) * progressPercent;
            }
        }

        private Vector3 GetNextPatrolTarget()
        {
            var remainingDistance = patrolHopDistance;

            while (remainingDistance > 0f)
            {
                var start = routePoints[routeSegmentIndex];
                var end = routePoints[(routeSegmentIndex + 1) % routePoints.Length];
                var segmentLength = Vector3.Distance(Flatten(start), Flatten(end));

                if (segmentLength <= 0.01f)
                {
                    routeSegmentIndex = (routeSegmentIndex + 1) % routePoints.Length;
                    routeSegmentProgress = 0f;
                    continue;
                }

                var distanceLeftOnSegment = segmentLength - routeSegmentProgress;

                if (remainingDistance <= distanceLeftOnSegment)
                {
                    routeSegmentProgress += remainingDistance;
                    remainingDistance = 0f;
                    continue;
                }

                remainingDistance -= distanceLeftOnSegment;
                routeSegmentIndex = (routeSegmentIndex + 1) % routePoints.Length;
                routeSegmentProgress = 0f;
            }

            var routeStart = routePoints[routeSegmentIndex];
            var routeEnd = routePoints[(routeSegmentIndex + 1) % routePoints.Length];
            var routeDirection = Flatten(routeEnd - routeStart).normalized;
            return routeStart + routeDirection * routeSegmentProgress;
        }

        private bool TryGetNavMeshPosition(Vector3 target, out Vector3 navMeshPosition)
        {
            if (NavMesh.SamplePosition(target, out var hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                navMeshPosition = hit.position;
                return true;
            }

            navMeshPosition = target;
            return false;
        }

        private static Vector3 Flatten(Vector3 position)
        {
            position.y = 0f;
            return position;
        }
    }
}
