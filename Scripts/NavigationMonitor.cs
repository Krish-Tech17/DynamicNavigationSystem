    using UnityEngine;
    using System;
    using System.Collections;

    public class NavigationMonitor : MonoBehaviour
    {
        public GuidedPathRenderer renderer;

        public float checkInterval = 0.5f;
        public float tolerance = 0.1f;  // How much increase triggers warning

        [Header("Debug")]
        public float totalDistance;     // baseline
        public float currentDistance;
        public bool warningTriggered;

        public event Action OnWrongDirection;
        public event Action OnBackOnTrack;

        private float timer;

        void Start()
        {
            if (!renderer)
                renderer = FindObjectOfType<GuidedPathRenderer>();

            StartCoroutine(DelayedInit()); // 1-second delay
        }

        void Update()
        {
            if (renderer.CurrentPath == null || renderer.CurrentPath.Count < 2)
                return;

            timer += Time.deltaTime;
            if (timer < checkInterval) return;
            timer = 0;

            CheckDistanceTrend();
        }

        IEnumerator DelayedInit()
        {
            float last = renderer.DistancePublicref;
            float stableTime = 0f;

            while (stableTime < 0.5f) // requires 0.5 seconds of stable value
            {
                yield return new WaitForSeconds(0.1f);

                float current = renderer.DistancePublicref;

                if (Mathf.Abs(current - last) < 0.05f)
                {
                    stableTime += 0.1f; // graph stable
                }
                else
                {
                    stableTime = 0f;    // unstable again → restart stability timer
                }

                last = current;
            }

            InitializeTracking();
            Debug.Log("Navigation tracking initialized after path stabilized.");
        }

        public void InitializeTracking()
        {
            totalDistance = renderer.DistancePublicref;
            warningTriggered = false;
        }

    void CheckDistanceTrend()
    {
        currentDistance = renderer.DistancePublicref;

        //  Moving in correct direction (towards exit)
        if (currentDistance < totalDistance)
        {
            totalDistance = currentDistance; // update baseline

            if (warningTriggered)
            {
                warningTriggered = false;
                Debug.Log(" Back on route");
                OnBackOnTrack?.Invoke();
            }

            return;
        }

        //  Moving in the wrong direction (distance increasing)
        if (currentDistance > totalDistance + tolerance)
        {
            // mark wrong direction
            if (!warningTriggered)
            {
                warningTriggered = true;
                Debug.Log(" WRONG WAY DETECTED!");
                OnWrongDirection?.Invoke();
            }

            // Update baseline to allow clearing warning if user comes back
            totalDistance = currentDistance;
        }
    }

}
