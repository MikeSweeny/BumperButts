using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtRocket : Powerup
{
    private float maxSpeed;
    private Transform[] waypoints;
    public Transform waypointHolder;
    private int currentWaypoint = 0;
    private float flightSpeed = 0.005f;

    public Vector3 location;

    // Start is called before the first frame update
    void Start()
    {
        GetWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
        location = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
        while (gameObject.transform.position != waypoints[currentWaypoint].position)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, waypoints[currentWaypoint].position, flightSpeed);
        }
        transform.LookAt(waypoints[currentWaypoint]);

        if (RelativeWaypointPosition.magnitude < 15)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }
    }

    public void GetWaypoints()
    {
        Transform[] potentialWaypoints = waypointHolder.GetComponentsInChildren<Transform>();
        waypoints = new Transform[(potentialWaypoints.Length - 1)];

        for (int i = 1; i < potentialWaypoints.Length; i++)
        {
            waypoints[i - 1] = potentialWaypoints[i];
        }
    }
}
