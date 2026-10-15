using System.Collections;
using UnityEngine;

// Health + explosion for an asteroid. When destroyed it may drop pickups, then respawns off screen
// (via Drift) so the field never runs out.
[RequireComponent(typeof(Drift))]
public class Asteroid : MonoBehaviour
{
    [SerializeField] int maxHealth = 1;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] Pickup[] dropTable; // empty = drops nothing
    [SerializeField] Vector2Int dropCount = new(1, 2);

    int health;
    SpriteRenderer sprite;
    Color baseColor;

    void Start()
    {
        health = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        baseColor = sprite.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health > 0)
        {
            GameAudio.Play("Hit", 0.5f);
            StartCoroutine(Flash());
            return;
        }

        GameAudio.Play("Explosion", 0.7f);
        if (explosionPrefab)
        {
            var boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            boom.transform.localScale = transform.localScale;
        }

        int drops = dropTable.Length == 0 ? 0 : Random.Range(dropCount.x, dropCount.y + 1);
        for (int i = 0; i < drops; i++)
        {
            var pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.6f);
            Instantiate(dropTable[Random.Range(0, dropTable.Length)], pos, Quaternion.identity);
        }

        health = maxHealth;
        sprite.color = baseColor;
        GetComponent<Drift>().Respawn();
    }

    IEnumerator Flash()
    {
        sprite.color = new Color(1f, 0.45f, 0.45f);
        yield return new WaitForSeconds(0.08f);
        sprite.color = baseColor;
    }
}
