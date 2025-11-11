using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(NavMeshSurface))]
public class ARNavMeshBuilder : MonoBehaviour
{
    public ARMeshManager meshManager;
    public float rebuildInterval = 2f;

    private NavMeshSurface _surface;
    private float _timer;

    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
    }

    private void OnEnable()
    {
        BuildNavMesh();
    }

    private void Update()
    {
        if (meshManager == null) return;

        _timer += Time.deltaTime;
        if (_timer >= rebuildInterval)
        {
            _timer = 0f;
            BuildNavMesh();
        }
    }

    public void BuildNavMesh()
    {
        _surface.BuildNavMesh();
    }
}
