using UnityEngine;

// Expands and fades out, then removes itself.
public class Explosion : MonoBehaviour
{
    [SerializeField] float duration = 0.4f;
    [SerializeField] float endScale = 2.5f;

    SpriteRenderer sprite;
    Vector3 startScale;
    float t;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime / duration;
        transform.localScale = startScale * Mathf.Lerp(1f, endScale, t);
        var c = sprite.color;
        c.a = 1f - t;
        sprite.color = c;
        if (t >= 1f) Destroy(gameObject);
    }
}
