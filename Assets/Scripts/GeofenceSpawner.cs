using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class GeofenceSpawner : MonoBehaviour
{
    [Header("Geofence (WGS84)")]
    public double spawnLat = 37.4219999; // example
    public double spawnLon = -122.0840575; // example
    public float spawnRadiusMeters = 20f; // enter this radius to spawn


    [Header("Spawn")]
    public GameObject blockPrefab;
    public bool spawnOnce = true;


    [Header("AR Foundation")]
    [Tooltip("Reference to the ARRaycastManager in your scene")]
    public ARRaycastManager raycastManager;


    [Header("Fallback Ground Detection")]
    [Tooltip("Layers treated as 'ground' when AR planes are not available.")]
    public LayerMask fallbackGroundLayers = ~0; // default: everything


    [Tooltip("Max distance to check for a ground collider below the camera.")]
    public float fallbackGroundCheckDistance = 10f;


    private bool _spawned;

    private void Start()
    {
        // TEMP: For indoor testing, spawn the block right away
        StartCoroutine(WaitForGroundAndSpawn());
    }

    private void OnEnable()
    {
       // if (GPSManager.Instance != null)
      //      GPSManager.Instance.OnLocation += HandleLocation;
    }


    private void OnDisable()
    {
    //    if (GPSManager.Instance != null)
    //        GPSManager.Instance.OnLocation -= HandleLocation;
    }


    private void HandleLocation(LocationInfo li)
    {
        if (_spawned && spawnOnce) return;


        float dMeters = HaversineMeters(li.latitude, li.longitude, spawnLat, spawnLon);
        if (dMeters <= spawnRadiusMeters)
        {
            StartCoroutine(WaitForGroundAndSpawn());
            _spawned = true;
        }
    }


    // Wait until AR Foundation detects a ground plane before spawning
    private IEnumerator WaitForGroundAndSpawn()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Give AR / scene one frame to initialize
        yield return null;

        while (true)
        {
            bool groundReady = false;

            // 1) Try AR plane via ARRaycastManager (if assigned)
            if (raycastManager != null)
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
                {
                    // If we want to use the actual AR hit pose, we could grab hits[0].pose here.
                    groundReady = true;
                }
            }

            // 2) Fallback: regular physics raycast down to any "ground" collider
            if (!groundReady)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    RaycastHit hit;
                    Vector3 origin = cam.transform.position + Vector3.up; // a bit above camera
                    if (Physics.Raycast(origin, Vector3.down, out hit, fallbackGroundCheckDistance, fallbackGroundLayers))
                    {
                        groundReady = true;
                    }
                }
            }

            if (groundReady)
            {
                SpawnNearUser();
                yield break;
            }

            // No ground yet; try again next frame
            yield return null;
        }
    }

    private void SpawnNearUser()
    {
        var cam = Camera.main;
        if (!cam) return;

        Vector3 pos = cam.transform.position + cam.transform.forward * 5.0f; // 5m in front
        Quaternion rot = Quaternion.identity;

        Instantiate(blockPrefab, pos, rot);
        Debug.Log("[GeofenceSpawner] Spawned NPC after plane detected!");
    }



    // Great-circle distance approximation (Haversine formula)
    private static float HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6378137.0; // Earth radius in meters

        double dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        double dLon = (lon2 - lon1) * Mathf.Deg2Rad;

        // Convert all to float-safe versions for Mathf
        float lat1f = (float)lat1;
        float lat2f = (float)lat2;
        float dLatf = (float)dLat;
        float dLonf = (float)dLon;

        double a =
            Mathf.Sin(dLatf / 2) * Mathf.Sin(dLatf / 2) +
            Mathf.Cos(lat1f * Mathf.Deg2Rad) *
            Mathf.Cos(lat2f * Mathf.Deg2Rad) *
            Mathf.Sin(dLonf / 2) * Mathf.Sin(dLonf / 2);

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        return (float)(R * c);
    }
}