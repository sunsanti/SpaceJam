using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tank-style controls: W/S thrust forward/back, A/D turn, Space shoots (hold for auto-fire).
// Shift dashes toward the held direction relative to the ship (Shift+A = dodge left; no key = forward).
// The sprite faces +X, so transform.right is forward and transform.up is the ship's left.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float thrust = 12f;
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float turnSpeed = 180f;

    [Header("Dash")]
    [SerializeField] float dashDistance = 3f;
    [SerializeField] float dashDuration = 0.12f;
    [SerializeField] float dashCooldown = 0.5f;

    [Header("Shooting")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireCooldown = 0.2f;
    [SerializeField] float muzzleOffset = 1.1f;

    [Header("Damage")]
    [SerializeField] int lives = 3;
    [SerializeField] float invulnerableTime = 2f;
    [SerializeField] Image[] hearts;
    [SerializeField] Sprite emptyHeart;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    float move, turn, nextDashTime, dashEndTime, nextFireTime;
    Vector2 dashVelocity;
    bool invulnerable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || Time.timeScale == 0) return; // ignore input while paused

        move = (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0);
        turn = (kb.aKey.isPressed ? 1 : 0) - (kb.dKey.isPressed ? 1 : 0);

        if (kb.shiftKey.wasPressedThisFrame && Time.time >= nextDashTime)
        {
            Vector2 dir = (Vector2)transform.right * move + (Vector2)transform.up * turn;
            if (dir == Vector2.zero) dir = transform.right;
            dashVelocity = dir.normalized * (dashDistance / dashDuration);
            dashEndTime = Time.time + dashDuration;
            nextDashTime = Time.time + dashCooldown;
        }

        if (kb.spaceKey.isPressed && Time.time >= nextFireTime)
        {
            Instantiate(bulletPrefab, transform.position + transform.right * muzzleOffset, transform.rotation);
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FixedUpdate()
    {
        if (Time.time < dashEndTime)
        {
            rb.linearVelocity = dashVelocity; // no turning mid-dash, so the dodge stays straight
            return;
        }

        rb.MoveRotation(rb.rotation + turn * turnSpeed * Time.fixedDeltaTime);
        rb.AddForce((Vector2)transform.right * move * thrust);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    // Asteroids are triggers, so the ship always passes through; it only takes damage when not blinking.
    void OnTriggerStay2D(Collider2D other)
    {
        if (invulnerable || !other.GetComponent<Drift>()) return; // only asteroids hurt (not our own bullets)

        lives--;
        if (lives >= 0 && lives < hearts.Length) hearts[lives].sprite = emptyHeart;
        if (lives <= 0)
        {
            SceneManager.LoadScene("MainMenu"); // temporary game over
            return;
        }
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        invulnerable = true;
        for (float t = 0; t < invulnerableTime; t += 0.1f)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.enabled = true;
        invulnerable = false;
    }
}
