using UnityEngine;

// Flies straight along its +X (the ship's nose direction). Hitting an asteroid recycles that asteroid off screen.
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 16f;
    [SerializeField] float lifetime = 1.2f;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var asteroid = other.GetComponent<Drift>();
        if (!asteroid) return;
        asteroid.Respawn();
        Destroy(gameObject);
    }
}
