using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARMeshManager))]
public class ARMeshColliders : MonoBehaviour
{
    void OnEnable()
    {
        var amm = GetComponent<ARMeshManager>();
        amm.meshesChanged += OnMeshesChanged;

        // existing meshes
        foreach (var mf in amm.meshes)
            EnsureCollider(mf);
    }

    void OnDisable()
    {
        var amm = GetComponent<ARMeshManager>();
        if (amm != null) amm.meshesChanged -= OnMeshesChanged;
    }

    void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        foreach (var mf in args.added) EnsureCollider(mf);
        foreach (var mf in args.updated) EnsureCollider(mf);
    }

    void EnsureCollider(MeshFilter mf)
    {
        var go = mf.gameObject;
        var col = go.GetComponent<MeshCollider>();
        if (!col) col = go.AddComponent<MeshCollider>();
        col.sharedMesh = mf.sharedMesh;
    }
}
