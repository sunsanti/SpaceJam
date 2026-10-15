using UnityEngine;

// Endless parallax star background. Needs a SpriteRenderer in Tiled draw mode, at least 6 tiles wide and tall
// (enough to cover the view even when the camera is rotated).
// parallax: 0 = stars move with the world, 1 = stars stuck to the camera.
[RequireComponent(typeof(SpriteRenderer))]
public class Starfield : MonoBehaviour
{
    [SerializeField, Range(0, 1)] float parallax = 0.7f;

    Transform cam;
    Vector2 tile;

    void Start()
    {
        cam = Camera.main.transform;
        tile = GetComponent<SpriteRenderer>().sprite.bounds.size;
    }

    void LateUpdate()
    {
        Vector2 c = cam.position;
        Vector2 scroll = c * (1 - parallax);
        transform.position = new Vector3(c.x - Mathf.Repeat(scroll.x, tile.x), c.y - Mathf.Repeat(scroll.y, tile.y), 0);
    }
}
