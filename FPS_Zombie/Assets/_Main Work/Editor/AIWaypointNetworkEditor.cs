using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor
{

    public override void OnInspectorGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;

        network.displayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode ", network.displayMode);

        if(network.displayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Waypoint Start", network.UIStart, 0, network.waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Waypoint End", network.UIEnd, 0, network.waypoints.Count - 1);
        }

        DrawDefaultInspector();
    }


    private void OnSceneGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;
        for (int i = 0; i < network.waypoints.Count; i++)
        {
            Handles.Label(network.waypoints[i].transform.position, "Waypoint " + i);
        }

        if(network.displayMode == PathDisplayMode.Connections)
        {
            Vector3[] linePoints = new Vector3[network.waypoints.Count + 1];

            for (int i = 0; i <= network.waypoints.Count; i++)
            {
                int index = i == network.waypoints.Count ? 0 : i;
                linePoints[i] = network.waypoints[index].position;
            }

            Handles.color = Color.cyan;
            Handles.DrawPolyLine(linePoints);

        }
        else if(network.displayMode == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();
            var from         = network.waypoints[network.UIStart].position;
            var to           = network.waypoints[network.UIEnd].position;
            NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);

            Handles.color = Color.yellow;
            Handles.DrawPolyLine(path.corners);
        }
    }

}
