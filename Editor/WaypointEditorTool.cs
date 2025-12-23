using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PathGroup))]
public class WaypointEditorTool : Editor
{
    private GUIStyle labelStyle;

    void OnSceneGUI()
    {
        PathGroup pg = (PathGroup)target;
        if (pg == null || pg.loopWaypoints == null) return;

        labelStyle ??= new GUIStyle()
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.white }
        };

        foreach (var wp in pg.loopWaypoints)
        {
            if (wp == null) continue;

            //------------------------------------------
            // Display waypoint index above sphere
            //------------------------------------------
            Handles.Label(
                wp.transform.position + Vector3.up * 0.4f,
                wp.name,
                labelStyle
            );

            //------------------------------------------
            // Draw neighbor connections
            //------------------------------------------
            foreach (var n in wp.neighbors)
            {
                if (n == null) continue;

                float d = Vector3.Distance(wp.transform.position, n.transform.position);

                // Color logic
                Handles.color = d > pg.connectRadius ? Color.red : Color.cyan;

                // Draw line
                Handles.DrawAAPolyLine(3f, wp.transform.position, n.transform.position);

                // Distance label
                Handles.Label((wp.transform.position + n.transform.position) * 0.5f, d.ToString("F2"));
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathGroup pg = (PathGroup)target;

        GUILayout.Space(10);

        if (GUILayout.Button(" Auto-Fill Waypoints"))
        {
            Undo.RecordObject(pg, "Auto Fill Waypoints");

            pg.loopWaypoints.Clear();
            foreach (Transform t in pg.transform)
            {
                Waypoint wp = t.GetComponent<Waypoint>();
                if (wp != null) pg.loopWaypoints.Add(wp);
            }

            EditorUtility.SetDirty(pg);
        }
    }
}
