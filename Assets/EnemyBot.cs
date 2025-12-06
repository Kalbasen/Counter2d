using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBot : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Units per second the bot moves toward its target.")]
    public float speed = 120f;

    [Tooltip("Offset from the starting position used as the center of the wandering area.")]
    public Vector2 wanderAreaCenter = Vector2.zero;

    [Tooltip("Half-size of the wandering area in world units.")]
    public Vector2 wanderAreaExtents = new Vector2(300f, 300f);

    [Tooltip("Distance from the target position before picking a new one.")]
    public float retargetDistance = 10f;

    [Tooltip("Seconds between automatic retargeting.")]
    public float retargetInterval = 1.5f;

    private Rigidbody2D rb;
    private Vector2 currentTarget;
    private Vector2 startPosition;
    private float nextRetargetTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        currentTarget = startPosition;
        ScheduleNextTarget();
    }

    private void FixedUpdate()
    {
        if (Time.time >= nextRetargetTime || TargetReached())
        {
            PickNewTarget();
        }

        MoveTowardsTarget();
    }

    private bool TargetReached()
    {
        float sqrDistance = ((Vector2)transform.position - currentTarget).sqrMagnitude;
        return sqrDistance <= retargetDistance * retargetDistance;
    }

    private void PickNewTarget()
    {
        Vector2 center = startPosition + wanderAreaCenter;
        Vector2 randomOffset = new Vector2(
            Random.Range(-wanderAreaExtents.x, wanderAreaExtents.x),
            Random.Range(-wanderAreaExtents.y, wanderAreaExtents.y));

        currentTarget = center + randomOffset;
        ScheduleNextTarget();
    }

    private void ScheduleNextTarget()
    {
        nextRetargetTime = Time.time + retargetInterval;
    }

    private void MoveTowardsTarget()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 direction = currentTarget - (Vector2)transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            rb.velocity = direction.normalized * speed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center = (Application.isPlaying ? startPosition : (Vector2)transform.position) + wanderAreaCenter;
        Vector3 areaCenter3D = new Vector3(center.x, center.y, 0f);
        Vector3 size = new Vector3(wanderAreaExtents.x * 2f, wanderAreaExtents.y * 2f, 0f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawCube(areaCenter3D, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter3D, size);
        Gizmos.DrawSphere(new Vector3(currentTarget.x, currentTarget.y, 0f), 5f);
    }
}
