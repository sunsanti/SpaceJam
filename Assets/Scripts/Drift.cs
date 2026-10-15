using UnityEngine;

// Slow random drift + spin. Wraps to the opposite screen edge once fully off screen.
// Speed scales with size, so small (far) rocks move slower.
public class Drift : MonoBehaviour
{
    [SerializeField] Vector2 speedRange = new(0.2f, 0.6f);
    [SerializeField] float maxSpin = 20f;

    Camera cam;
    Vector3 velocity;
    float spin, margin;

    void Start()
    {
        cam = Camera.main;
        velocity = Random.insideUnitCircle.normalized * Random.Range(speedRange.x, speedRange.y) * transform.localScale.x;
        spin = Random.Range(-maxSpin, maxSpin);
        margin = GetComponent<SpriteRenderer>().bounds.extents.magnitude;
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(0, 0, spin * Time.deltaTime);

        // Wrap in camera space so it still works when the camera rotates.
        float h = cam.orthographicSize + margin, w = cam.orthographicSize * cam.aspect + margin;
        Vector3 p = cam.transform.InverseTransformPoint(transform.position);
        if (p.x > w) p.x -= 2 * w; else if (p.x < -w) p.x += 2 * w;
        if (p.y > h) p.y -= 2 * h; else if (p.y < -h) p.y += 2 * h;
        SetCameraLocal(p);
    }

    // Moves the asteroid to a random point just outside the camera view.
    public void Respawn()
    {
        float h = cam.orthographicSize + margin, w = cam.orthographicSize * cam.aspect + margin;
        Vector2 p = Random.value < 0.5f
            ? new Vector2(Random.value < 0.5f ? -w : w, Random.Range(-h, h))
            : new Vector2(Random.Range(-w, w), Random.value < 0.5f ? -h : h);
        SetCameraLocal(p);
    }

    void SetCameraLocal(Vector2 p)
    {
        Vector3 world = cam.transform.TransformPoint(p.x, p.y, 0);
        transform.position = new Vector3(world.x, world.y, 0f);
    }
}
