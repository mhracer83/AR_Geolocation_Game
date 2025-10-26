using System;
using System.Collections;
using UnityEngine;


public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { get; private set; }


    [Header("Settings")]
    [Tooltip("How accurate the GPS must be (meters) before we trust it.")]
    public float requiredAccuracyMeters = 25f;


    [Tooltip("How long to wait for GPS to initialize (seconds).")]
    public int initTimeoutSeconds = 20;


    public bool IsReady { get; private set; }
    public bool HasFix => Input.location.status == LocationServiceStatus.Running;


    public LocationInfo Current => Input.location.lastData;


    public event Action<LocationInfo> OnLocation;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private IEnumerator Start()
    {
        // Permission request (Android); iOS uses plist strings
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("GPS disabled by user.");
            yield break;
        }


        Input.location.Start(1f, 0.5f); // desiredAccuracyMeters, updateDistanceMeters


        // Wait for init
        int waited = 0;
        while (Input.location.status == LocationServiceStatus.Initializing && waited < initTimeoutSeconds)
        {
            yield return new WaitForSeconds(1f);
            waited++;
        }
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("GPS failed to start or timed out.");
            yield break;
        }


        IsReady = true;
        StartCoroutine(LocationPump());
    }


    private IEnumerator LocationPump()
    {
        var lastTimestamp = -1d;
        while (true)
        {
            if (HasFix)
            {
                var li = Current;
                if (li.timestamp != lastTimestamp && li.horizontalAccuracy <= requiredAccuracyMeters)
                {
                    lastTimestamp = li.timestamp;
                    OnLocation?.Invoke(li);
                }
            }
            yield return null;
        }
    }
}