using UnityEngine;
using System.Collections.Generic;

public class ArrowPathRenderer : MonoBehaviour
{
    [Header("Arrow Prefab Setup")]
    public GameObject arrowPrefab;

    [Header("Visual Settings")]
    [Range(0.1f, 5f)]
    public float arrowScale = 1f;

    [Range(0f, 0.5f)]
    public float floatHeight = 0.05f;

    [Tooltip("Rotate arrow if it points the wrong way (Y rotation mostly).")]
    public Vector3 rotationOffsetEuler = new Vector3(0, 0, 0);

    [Header("Path Settings")]
    [Range(0.1f, 2f)]
    public float spacing = 0.5f;

    [Tooltip("Reverse Direction: Arrows point back toward player instead of toward exit.")]
    public bool reverseDirection = false;

    [Header("Animation")]
    public bool animate = true;
    [Range(0f, 4f)]
    public float animationSpeed = 2f;
    [Range(0f, 1f)]
    public float pulseAmount = 0.3f;

    [Header("Debug Options")]
    public bool autoRefresh = true;
    public bool showGizmoDirection = true;

    private readonly List<GameObject> arrows = new List<GameObject>();


    // PUBLIC — called from PathGuide
    public void RenderPath(List<Waypoint> path)
    {
        // Disable old arrows before rebuilding
        for (int i = 0; i < arrows.Count; i++)
            arrows[i].SetActive(false);

        if (path == null || path.Count < 2 || arrowPrefab == null)
            return;

        Quaternion offsetRot = Quaternion.Euler(rotationOffsetEuler);
        int arrowIndex = 0;
        float animOffset = 0f;

        // Loop path segments
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i].transform.position;
            Vector3 end = path[i + 1].transform.position;

            Vector3 dir = (end - start);
            Vector3 flatDir = new Vector3(dir.x, 0, dir.z);
            float segmentLength = flatDir.magnitude;

            if (segmentLength < spacing) continue;

            flatDir.Normalize();
            if (reverseDirection) flatDir = -flatDir;

            int count = Mathf.FloorToInt(segmentLength / spacing);

            for (int j = 0; j < count; j++)
            {
                Vector3 pos = start + (flatDir * j * spacing);

                pos = GetGroundPoint(pos);
                pos.y += floatHeight;

                GameObject arrow;
                if (arrowIndex < arrows.Count)
                    arrow = arrows[arrowIndex];
                else
                {
                    arrow = Instantiate(arrowPrefab, transform);
                    arrows.Add(arrow);
                }

                arrow.transform.position = pos;
                arrow.transform.rotation = Quaternion.LookRotation(flatDir) * offsetRot;

                // scaling
                arrow.transform.localScale = Vector3.one * arrowScale;

                // pulse animation
                if (animate)
                {
                    float t = (Time.time * animationSpeed) + animOffset;
                    float s = 1f + Mathf.Sin(t) * pulseAmount;
                    arrow.transform.localScale = Vector3.one * arrowScale * s;
                }

                arrow.SetActive(true);
                arrowIndex++;
                animOffset += 0.4f;
            }
        }
    }


    // optional ground raycast
    Vector3 GetGroundPoint(Vector3 pos)
    {
        if (Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, out var hit, 10f))
            return hit.point;

        return pos;
    }


    // Live editor helper for debugging
    private void OnDrawGizmos()
    {
        if (!showGizmoDirection || arrows == null) return;

        Gizmos.color = Color.yellow;
        foreach (var a in arrows)
        {
            if (a && a.activeSelf)
                Gizmos.DrawRay(a.transform.position, a.transform.forward * 0.5f);
        }
    }
}
