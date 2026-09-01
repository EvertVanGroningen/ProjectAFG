using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base movement speed in units/second")]
    public float baseSpeed = 5f;
    [Tooltip("Acceleration when input is held")]
    public float acceleration = 25f;
    [Tooltip("Deceleration when input is released")]
    public float deceleration = 35f;
    [Tooltip("Multiplier applied to baseSpeed (can be changed at runtime to make speed dynamic)")]
    [Range(0f, 5f)]
    public float speedMultiplier = 1f;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    // internal state
    Vector2 currentVelocity;
    Vector2 inputVector;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Read raw input for sharp responsive control (suitable for fighting games)
        float hx = Input.GetAxisRaw("Horizontal");
        float vy = Input.GetAxisRaw("Vertical");
        inputVector = new Vector2(hx, vy);

        // Flip sprite based on horizontal input
        if (hx != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(hx) * Mathf.Abs(s.x);
            transform.localScale = s;
        }

        // Animator control (parameters used below must exist in the Animator)
        if (animator != null)
        {
            // Speed is absolute horizontal speed used for e.g. run animations
            animator.SetFloat("Speed", Mathf.Abs(currentVelocity.x));
            // Vertical velocity (useful for jump/fall states)
            animator.SetFloat("VelocityY", currentVelocity.y);
            // Simple moving flag
            animator.SetBool("IsMoving", Mathf.Abs(inputVector.x) > 0.01f);
        }
    }

    void FixedUpdate()
    {
        // Determine target velocity based on input and dynamic speed
        float targetSpeed = baseSpeed * Mathf.Max(0f, speedMultiplier);
        Vector2 targetVelocity = inputVector.normalized * targetSpeed;

        // Smoothly move current velocity toward target velocity
        float rate = (inputVector.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        // Apply to Rigidbody2D if present, otherwise move transform
        if (rb != null)
        {
            rb.velocity = currentVelocity;
        }
        else
        {
            transform.Translate(currentVelocity * Time.fixedDeltaTime);
        }
    }

    // Public API to change movement speed dynamically at runtime
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void AddToSpeedMultiplier(float delta)
    {
        speedMultiplier = Mathf.Max(0f, speedMultiplier + delta);
    }
}
