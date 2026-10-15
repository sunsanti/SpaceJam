using System.Collections;
using UnityEngine;

// Flies straight across the screen from one edge to the other, waits, then repeats on a new random route.
// The camera clips the sprite at the screen edge, so the ship slides in/out naturally.
public class ShipFlyBy : MonoBehaviour
{
    [SerializeField] Vector2 speedRange = new(3f, 6f);
    [SerializeField] Vector2 waitRange = new(1f, 4f);
    [SerializeField] float maxTilt = 15f;

    IEnumerator Start()
    {
        var cam = Camera.main;
        var trail = GetComponentInChildren<TrailRenderer>();
        float margin = GetComponent<SpriteRenderer>().bounds.extents.magnitude;

        while (true)
        {
            float h = cam.orthographicSize, w = h * cam.aspect;
            float dirX = Random.value < 0.5f ? 1f : -1f;
            Vector3 center = cam.transform.position;
            transform.position = new Vector3(center.x - dirX * (w + margin), center.y + Random.Range(-h, h) * 0.8f, 0f);
            transform.right = Quaternion.Euler(0, 0, Random.Range(-maxTilt, maxTilt)) * new Vector3(dirX, 0, 0);
            if (trail) trail.Clear();

            float speed = Random.Range(speedRange.x, speedRange.y);
            while ((transform.position.x - center.x) * dirX <= w + margin)
            {
                transform.position += transform.right * speed * Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(waitRange.x, waitRange.y));
        }
    }
}
