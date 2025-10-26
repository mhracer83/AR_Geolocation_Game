using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class BlockChaser : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.5f; // m/s
    public float stopDistance = 1.2f; // don’t overlap the user
    public float steeringAvoidStrength = 2.0f; // strength of obstacle sidestep
    public float obstacleProbeDistance = 1.0f; // raycast distance


    [Header("Grounding")]
    public float gravity = -9.81f;


    private CharacterController _cc;
    private Vector3 _velocity;


    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (_cc.height < 0.3f) _cc.height = 0.3f; // small block collider
        if (_cc.radius < 0.15f) _cc.radius = 0.15f;
    }


    private void Update()
    {
        var cam = Camera.main;
        if (!cam) return;


        Vector3 toUser = cam.transform.position - transform.position;
        Vector3 toUserXZ = Vector3.ProjectOnPlane(toUser, Vector3.up);
        float dist = toUserXZ.magnitude;


        Vector3 desired = Vector3.zero;
        if (dist > stopDistance)
            desired = toUserXZ.normalized * speed;


        // Simple obstacle avoidance via raycasts in forward/left/right
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -45, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 45, 0) * forward;


        desired += AvoidDir(forward) + AvoidDir(left) + AvoidDir(right);


        // Move & face
        _velocity.x = desired.x;
        _velocity.z = desired.z;
        _velocity.y += gravity * Time.deltaTime;


        _cc.Move(_velocity * Time.deltaTime);


        Vector3 look = new Vector3(desired.x, 0, desired.z);
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 8f * Time.deltaTime);
    }


    private Vector3 AvoidDir(Vector3 dir)
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, obstacleProbeDistance))
        {
            // steer away from obstacle normal (sidestep)
            Vector3 away = Vector3.Cross(hit.normal, Vector3.up).normalized; // lateral
            return away * steeringAvoidStrength;
        }
        return Vector3.zero;
    }
}