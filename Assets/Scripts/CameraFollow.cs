using UnityEngine;

// Smoothly follows the target. With rotateWithTarget on, the camera also turns so the ship's nose
// (its +X) always points up the screen and the world rotates around the ship.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.15f;
    [SerializeField] bool rotateWithTarget = true;
    [SerializeField] float rotateSmoothTime = 0.1f;

    Vector3 velocity;
    float angleVelocity;

    void LateUpdate()
    {
        if (!target) return;

        var goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);

        if (rotateWithTarget)
        {
            float goalAngle = target.eulerAngles.z - 90f;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, goalAngle, ref angleVelocity, rotateSmoothTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
