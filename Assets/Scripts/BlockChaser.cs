using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BlockChaser : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.5f;          // meters per second
    public float stopDistance = 2.0f;   // how far from user to stop (front of block)

    [Header("Grounding")]
    public float gravity = -9.81f;

    private CharacterController _cc;
    private Vector3 _velocity;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // Make sure the collider roughly matches the visible block.
        // Adjust these numbers to match your cube size.
        if (_cc.height < 1.0f) _cc.height = 1.0f;
        if (_cc.radius < 0.3f) _cc.radius = 0.3f;
        _cc.center = new Vector3(0f, _cc.height * 0.5f, 0f);
    }

    private void Update()
    {
        var cam = Camera.main;
        if (!cam) return;

        // Direction to user on XZ plane
        Vector3 toUser = cam.transform.position - transform.position;
        Vector3 toUserXZ = Vector3.ProjectOnPlane(toUser, Vector3.up);
        float dist = toUserXZ.magnitude;

        float desiredStopDistance = stopDistance + _cc.radius;

        Vector3 moveDir = Vector3.zero;

        if (dist > desiredStopDistance)
        {
            moveDir = toUserXZ.normalized;
        }

        // Constant-speed horizontal movement
        Vector3 horizontal = Vector3.zero;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            horizontal = moveDir * speed;
        }

        _velocity.x = horizontal.x;
        _velocity.z = horizontal.z;
        _velocity.y += gravity * Time.deltaTime;

        _cc.Move(_velocity * Time.deltaTime);

        // Always look at the player so it "tracks you with its eyes"
        Vector3 lookDir = toUserXZ;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                8f * Time.deltaTime
            );
        }
    }
}
