using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction { Up, Down, Left, Right }

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private FixedJoystick joystickMove;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Direction currentDirection = Direction.Down;

    [Header("Combat")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpread = 5f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private CircleCollider2D detectionCollider;
    private bool canShoot = true;
    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
            detectionCollider.radius = attackRange;
            Debug.Log("Player detection collider radius: " + detectionCollider.radius);
        }
    }

    private void Update()
    {
        movement.x = joystickMove.Horizontal;
        movement.y = joystickMove.Vertical;

        if (movement.magnitude > 0.1f)
        {
            DetermineDirection(movement);
        }

        if (enemiesInRange.Count > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestEnemy = null;

            foreach (Transform enemy in enemiesInRange)
            {
                if (enemy == null) continue;

                float distance = Vector2.Distance(transform.position, enemy.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            currentTarget = closestEnemy;

            if (currentTarget != null && canShoot)
            {
                Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
                DetermineDirection(directionToTarget);
                StartCoroutine(Shoot(directionToTarget));
                Debug.Log("Player shooting at: " + currentTarget.name);
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    private void FixedUpdate()
    {
        if (movement.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void DetermineDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            currentDirection = direction.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            currentDirection = direction.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private IEnumerator Shoot(Vector2 direction)
    {
        canShoot = false;

        float spreadAngle = Random.Range(-bulletSpread, bulletSpread);
        Quaternion spreadRotation = Quaternion.Euler(0, 0, spreadAngle);
        Vector2 finalDirection = spreadRotation * direction;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(finalDirection, "Player");

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took damage: " + damage + ", HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
                Debug.Log("Player detected enemy: " + other.name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.transform);
            Debug.Log("Player lost enemy: " + other.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, currentTarget.position);
        }
    }
}