using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tank-style controls: W/S thrust forward/back, A/D turn, Space shoots (hold for auto-fire).
// Shift dashes toward the held direction relative to the ship (Shift+A = dodge left; no key = forward).
// The sprite faces +X, so transform.right is forward and transform.up is the ship's left.
// Pickups: ExtraShot upgrades the gun 1 -> 2 -> 3 beams -> V-spread with double damage; Shield blocks one hit; Heal +1 life.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float thrust = 20f;
    [SerializeField] float maxSpeed = 11f;
    [SerializeField] float turnSpeed = 200f;

    [Header("Dash")]
    [SerializeField] float dashDistance = 3f;
    [SerializeField] float dashDuration = 0.12f;
    [SerializeField] float dashCooldown = 0.5f;

    [Header("Shooting")]
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] float fireCooldown = 0.2f;
    [SerializeField] float muzzleOffset = 1.1f;
    [SerializeField, Range(1, MaxWeaponLevel)] int weaponLevel = 1;
    [SerializeField] Color upgradedBulletColor = new(1f, 0.6f, 0.2f);

    [Header("Damage")]
    [SerializeField] int lives = 3;
    [SerializeField] float invulnerableTime = 2f;
    [SerializeField] Image[] hearts;
    [SerializeField] Sprite emptyHeart;
    [SerializeField] GameObject shield;

    const int MaxWeaponLevel = 4;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    float move, turn, nextDashTime, dashEndTime, nextFireTime;
    Vector2 dashVelocity;
    bool invulnerable;
    Sprite fullHeart;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        fullHeart = hearts.Length > 0 ? hearts[0].sprite : null;
        shield.SetActive(false);
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
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Fire()
    {
        switch (weaponLevel)
        {
            case 1: Shoot(0f, 0f); break;
            case 2: Shoot(0.25f, 0f); Shoot(-0.25f, 0f); break;
            case 3: Shoot(0.4f, 0f); Shoot(0f, 0f); Shoot(-0.4f, 0f); break;
            default: // V-spread: wings angle outward
                Shoot(0f, 0f);
                Shoot(0.3f, 12f); Shoot(-0.3f, -12f);
                Shoot(0.6f, 24f); Shoot(-0.6f, -24f);
                break;
        }
    }

    // side: offset toward the ship's left (+) / right (-); angle: degrees turned toward the left (+)
    void Shoot(float side, float angle)
    {
        var pos = transform.position + transform.right * muzzleOffset + transform.up * side;
        var bullet = Instantiate(bulletPrefab, pos, transform.rotation * Quaternion.Euler(0, 0, angle));
        if (weaponLevel == MaxWeaponLevel)
        {
            bullet.damage = 2;
            bullet.GetComponent<SpriteRenderer>().color = upgradedBulletColor;
        }
    }

    public void ApplyPickup(PickupType type)
    {
        switch (type)
        {
            case PickupType.ExtraShot:
                weaponLevel = Mathf.Min(weaponLevel + 1, MaxWeaponLevel);
                break;
            case PickupType.Shield:
                shield.SetActive(true);
                break;
            case PickupType.Heal:
                if (lives < hearts.Length)
                {
                    hearts[lives].sprite = fullHeart;
                    lives++;
                }
                break;
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
    // An active shield absorbs the hit instead of a life.
    void OnTriggerStay2D(Collider2D other)
    {
        if (invulnerable || !other.GetComponent<Asteroid>()) return; // only asteroids hurt (not bullets or pickups)

        if (shield.activeSelf)
        {
            shield.SetActive(false);
            StartCoroutine(Blink());
            return;
        }

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
