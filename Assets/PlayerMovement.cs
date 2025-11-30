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
    [Range(0f, 30f)]
    public float swayAmplitude = 8f;
    [Tooltip("How fast the sway oscillates.")]
    [Range(0.1f, 20f)]
    public float swayFrequency = 6f;
    [Tooltip("How quickly the tilt follows the target sway.")]
    [Range(0.1f, 30f)]
    public float swaySmooth = 8f;
    [Tooltip("Minimum joystick magnitude to start swaying.")]
    [Range(0f, 1f)]
    public float swayDeadzone = 0.05f;

    [Header("Bobbing (vertical) settings")]
    [Tooltip("Max vertical bob (units) applied to sprite local position.")]
    [Range(0f, 1f)]
    public float bobAmplitude = 0.08f;
    [Tooltip("How fast the bob oscillates.")]
    [Range(0.1f, 20f)]
    public float bobFrequency = 6f;
    [Tooltip("How quickly the bob follows the target value.")]
    [Range(0.1f, 30f)]
    public float bobSmooth = 8f;

    private float currentSway = 0f;
    private float currentBob = 0f;
    private Vector3 spriteRootInitialLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRoot == null)
        {
            // If not assigned, use self so rotation still applies
            spriteRoot = transform;
        }
        if (spriteRoot != null)
            spriteRootInitialLocalPos = spriteRoot.localPosition;
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
        ApplySwayAndBob();
    }

    void ApplySwayAndBob()
    {
        if (spriteRoot == null) return;

        // intensity: how much the player is moving (0..1). use full joystick magnitude so vertical moves affect equally
        float intensity = Mathf.Clamp01(direction.magnitude);

        if (intensity <= swayDeadzone)
        {
            // smoothly return to neutral when not moving
            currentSway = Mathf.Lerp(currentSway, 0f, Time.deltaTime * swaySmooth);
            currentBob = Mathf.Lerp(currentBob, 0f, Time.deltaTime * bobSmooth);
        }
        else
        {
            // determine sway sign: prefer horizontal direction, fall back to vertical sign so movement up/down still produces sway
            float signSource = Mathf.Abs(direction.x) > 0.01f ? direction.x : (direction.y != 0f ? direction.y : 1f);
            float sign = Mathf.Sign(signSource);

            // generate a sinusoidal sway that depends on time and intensity
            float targetSway = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude * intensity * sign;
            currentSway = Mathf.Lerp(currentSway, targetSway, Time.deltaTime * swaySmooth);

            // generate vertical bob (подпрыгивание)
            float targetBob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude * intensity;
            currentBob = Mathf.Lerp(currentBob, targetBob, Time.deltaTime * bobSmooth);
        }

        // apply rotation around Z (2D)
        spriteRoot.localRotation = Quaternion.Euler(0f, 0f, currentSway);

        // apply local position bob on Y while preserving initial X/Z
        Vector3 p = spriteRootInitialLocalPos;
        p.y += currentBob;
        spriteRoot.localPosition = p;
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