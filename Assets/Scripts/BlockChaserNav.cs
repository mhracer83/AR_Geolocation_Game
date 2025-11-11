using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BlockChaserNav : MonoBehaviour
{
    public float extraStopDistance = 0.5f; // extra buffer past agent.stoppingDistance

    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false; // we rotate manually
    }

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 playerPos = cam.transform.position;
        Vector3 toUser = playerPos - transform.position;
        Vector3 toUserXZ = Vector3.ProjectOnPlane(toUser, Vector3.up);
        float dist = toUserXZ.magnitude;

        float desiredStop = _agent.stoppingDistance + extraStopDistance;

        if (dist > desiredStop)
        {
            _agent.SetDestination(playerPos);
        }
        else
        {
            if (_agent.hasPath)
                _agent.ResetPath();
        }

        // Face the player
        if (toUserXZ.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(toUserXZ);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target,
                8f * Time.deltaTime
            );
        }
    }
}
