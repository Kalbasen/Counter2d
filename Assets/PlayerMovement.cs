using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    public Joystick joystick;
    public float speed = 5f;

    [Header("Physics")]
    private Rigidbody2D rb;
    private Vector2 direction;

    [Header("Sway (tilt) settings")]
    [Tooltip("Transform which will be rotated for the sway effect (usually the sprite GameObject).")]
    public Transform spriteRoot;
    [Tooltip("Max degrees of tilt/sway.")]
    public float swayAmplitude = 8f;
    [Tooltip("How fast the sway oscillates.")]
    public float swayFrequency = 6f;
    [Tooltip("How quickly the tilt follows the target sway.")]
    public float swaySmooth = 8f;
    [Tooltip("Minimum joystick magnitude to start swaying.")]
    public float swayDeadzone = 0.05f;

    private float currentSway = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRoot == null)
        {
            // If not assigned, try to use first child or self
            spriteRoot = transform;
        }
    }

    void FixedUpdate()
    {
        // Read joystick direction
        if (joystick != null) direction = joystick.Direction;
        else direction = Vector2.zero;

        // Apply movement via Rigidbody2D velocity
        if (rb != null)
            rb.velocity = direction * speed;
    }

    void Update()
    {
        ApplySway();
    }

    void ApplySway()
    {
        if (spriteRoot == null) return;

        // intensity: how much the player is moving (0..1). using horizontal magnitude produces left/right sway.
        float intensity = Mathf.Clamp01(Mathf.Abs(direction.x));

        if (intensity <= swayDeadzone)
        {
            // smoothly return to neutral when not moving
            currentSway = Mathf.Lerp(currentSway, 0f, Time.deltaTime * swaySmooth);
        }
        else
        {
            // generate a sinusoidal sway that depends on time and intensity
            float target = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude * intensity * Mathf.Sign(direction.x);
            // Use sign(direction.x) so when moving left the sway is mirrored
            currentSway = Mathf.Lerp(currentSway, target, Time.deltaTime * swaySmooth);
        }

        // apply rotation around Z (2D)
        spriteRoot.localRotation = Quaternion.Euler(0f, 0f, currentSway);
    }

    // Optional: draw debug
    void OnDrawGizmosSelected()
    {
        if (spriteRoot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spriteRoot.position, 0.1f);
        }
    }
}