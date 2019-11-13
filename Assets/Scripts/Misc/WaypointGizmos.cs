using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{

    public float size = 1f;
    private Transform[] waypoints;

    private void OnDrawGizmos()
    {
        waypoints = gameObject.GetComponentsInChildren<Transform>();
        Vector3 last = waypoints[waypoints.Length - 1].position;
        int n = waypoints.Length;
        for (int i = 1; i < n; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[i].position, size);
            Gizmos.DrawLine(last, waypoints[i].position);
            last = waypoints[i].position;
        }
    }

}
