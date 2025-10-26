using UnityEngine;


public class GeofenceSpawner : MonoBehaviour
{
    [Header("Geofence (WGS84)")]
    public double spawnLat = 37.4219999; // example
    public double spawnLon = -122.0840575; // example
    public float spawnRadiusMeters = 20f; // enter this radius to spawn


    [Header("Spawn")]
    public GameObject blockPrefab;
    public bool spawnOnce = true;


    private bool _spawned;

    private void Start()
    {
        // TEMP: For indoor testing, spawn the block right away
        SpawnNearUser();
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
            SpawnNearUser();
            _spawned = true;
        }
    }


    private void SpawnNearUser()
    {
        var cam = Camera.main;
        if (!cam) return;
        Vector3 pos = cam.transform.position + cam.transform.forward * 2.0f; // 2m in front
        Quaternion rot = Quaternion.identity;
        Instantiate(blockPrefab, pos, rot);
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