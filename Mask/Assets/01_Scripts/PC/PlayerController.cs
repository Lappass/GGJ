using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 4f;

    [Header("Sprint")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float sprintSpeed = 7f;

    [Header("Crouch")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private bool crouchHold = true;
    [SerializeField] private float crouchSpeedMultiplier = 0.6f;
    [SerializeField] private float crouchHeightMultiplier = 0.6f;
    [SerializeField] private float crouchLerpSpeed = 12f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    [Header("Anim")]
    [SerializeField] private Animator anim;
    private Vector2 input;
    private bool isCrouching;
    private SpriteRenderer sr;

    private Vector2 standSize;
    private Vector2 standOffset;

    private bool _canControl = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        standSize = col.size;
        standOffset = col.offset;
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnEnable()
    {
        DialogueManager.OnGlobalDialogueStart += OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd += OnDialogueEnd;
    }

    private void OnDisable()
    {
        DialogueManager.OnGlobalDialogueStart -= OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd -= OnDialogueEnd;
        // Ensure no residual movement when disabled
        input = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    private void OnDialogueStart()
    {
        _canControl = false;
        input = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    private void OnDialogueEnd()
    {
        _canControl = true;
    }

    private void Update()
    {
        if (!_canControl) return;

        //WASD
        float x = 0f;
        float y = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;
        input = new Vector2(x, y);
        input = Vector2.ClampMagnitude(input, 1f);

        //Crouch
        if (crouchHold)
        {
            isCrouching = Input.GetKey(crouchKey);
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
                isCrouching = !isCrouching;
        }
        if (sr != null && Mathf.Abs(input.x) > 0.01f)
            sr.flipX = input.x > 0;
        if (anim != null) anim.SetFloat("Speed", input.magnitude);

    }

    private void FixedUpdate()
    {
        float targetSpeed = speed;

        // Sprint
        bool hasMove = input.sqrMagnitude > 0.0001f;
        bool sprinting = hasMove && Input.GetKey(sprintKey) && !isCrouching;
        if (sprinting) targetSpeed = sprintSpeed;

        // Crouch speed
        if (isCrouching)
            targetSpeed *= crouchSpeedMultiplier;

        rb.MovePosition(rb.position + input * targetSpeed * Time.fixedDeltaTime);

        ApplyCrouchCollider();
    }

    private void ApplyCrouchCollider()
    {
        float t = crouchLerpSpeed * Time.fixedDeltaTime;
        float targetHeight = standSize.y * (isCrouching ? crouchHeightMultiplier : 1f);
        Vector2 size = col.size;
        size.y = Mathf.Lerp(size.y, targetHeight, t);
        col.size = size;
        float standBottom = standOffset.y - standSize.y / 2f;
        Vector2 off = col.offset;
        off.y = standBottom + col.size.y / 2f;
        col.offset = off;
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        sprintSpeed = Mathf.Max(0f, sprintSpeed);
        crouchSpeedMultiplier = Mathf.Clamp(crouchSpeedMultiplier, 0.05f, 1f);
        crouchHeightMultiplier = Mathf.Clamp(crouchHeightMultiplier, 0.2f, 1f);
        crouchLerpSpeed = Mathf.Max(0.01f, crouchLerpSpeed);
    }
}
