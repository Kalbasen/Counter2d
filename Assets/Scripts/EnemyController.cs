using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float minDistance = 3f; 
    [SerializeField] private float maxDistance = 7f; 
    private Rigidbody2D rb;
    private Vector2 movement;
    private Direction currentDirection = Direction.Down;

    [Header("Combat")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpread = 8f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private CircleCollider2D detectionCollider;
    private bool canShoot = true;

    [Header("Strafe Settings")]
    [SerializeField] private float strafeDirectionChangeTime = 2f; 
    [SerializeField] private bool strafeClockwise = true;
    private float strafeTimer = 0f;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;


    private Transform playerTarget;
    private bool playerDetected = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
            detectionCollider.radius = detectionRange;
        }

        FindPlayer();


        strafeClockwise = Random.value > 0.5f;
        strafeTimer = Random.Range(0f, strafeDirectionChangeTime);
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (playerDetected)
        {

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            DetermineDirection(directionToPlayer);

            CalculateStrafeMovement(distanceToPlayer, directionToPlayer);

            if (canShoot && distanceToPlayer <= detectionRange)
            {
                StartCoroutine(Shoot(directionToPlayer));
            }
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (movement.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void CalculateStrafeMovement(float distanceToPlayer, Vector2 directionToPlayer)
    {
        Vector2 distanceMovement = Vector2.zero;

        if (distanceToPlayer > maxDistance)
        {
            distanceMovement = directionToPlayer * 0.7f;
        }
        else if (distanceToPlayer < minDistance)
        {
         
            distanceMovement = -directionToPlayer * 0.7f;
        }
        else
        {
            distanceMovement = Vector2.zero;
        }

        Vector2 strafeDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x);

    
        strafeTimer += Time.deltaTime;
        if (strafeTimer >= strafeDirectionChangeTime)
        {
            strafeTimer = 0f;
            strafeClockwise = !strafeClockwise; 
        }

     
        if (!strafeClockwise)
        {
            strafeDirection = -strafeDirection; 
        }

       
        float strafeIntensity = Mathf.Clamp((distanceToPlayer - stoppingDistance) / stoppingDistance, 0.5f, 1f);
        Vector2 strafeMovement = strafeDirection * strafeIntensity;

    
        movement = (distanceMovement + strafeMovement).normalized;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
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
        bulletScript.Initialize(finalDirection, "Enemy");

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}