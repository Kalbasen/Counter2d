using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;
    private string ownerTag;
    private Rigidbody2D rb;
    private Vector2 direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 shootDirection, string owner)
    {
        direction = shootDirection.normalized;
        ownerTag = owner;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.isTrigger) return;


        if (collision.CompareTag(ownerTag)) return;


        if (collision.CompareTag("Player") && ownerTag == "Enemy")
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy") && ownerTag == "Player")
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null) enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    
        else if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}