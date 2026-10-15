using UnityEngine;

// Flies straight along its +X (the ship's nose direction) and damages the first asteroid it hits.
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 18f;
    [SerializeField] float lifetime = 1.2f;

    public int damage = 1;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var asteroid = other.GetComponent<Asteroid>();
        if (!asteroid) return;
        asteroid.TakeDamage(damage);
        Destroy(gameObject);
    }
}
