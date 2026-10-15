using UnityEngine;

public enum PickupType { ExtraShot, Shield, Heal }

// Item bubble: drifts slowly and pulses, blinks during its last seconds, then disappears.
// The player ship collects it on contact.
public class Pickup : MonoBehaviour
{
    [SerializeField] PickupType type;
    [SerializeField] float lifetime = 12f;

    SpriteRenderer sprite;
    Vector3 velocity;
    float age;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        velocity = Random.insideUnitCircle * 0.6f;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(age * 4f));
        sprite.enabled = lifetime - age > 3f || Mathf.Repeat(age, 0.3f) < 0.15f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ship = other.GetComponent<PlayerShip>();
        if (!ship) return;
        ship.ApplyPickup(type);
        Destroy(gameObject);
    }
}
