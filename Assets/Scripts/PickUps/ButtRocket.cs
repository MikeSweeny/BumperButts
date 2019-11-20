using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtRocket : Powerup
{
    private float maxSpeed;
    private Transform[] waypoints;
    public Transform waypointHolder;
    public int currentWaypoint = 0;
    private float flightSpeed = 25f;

    private Vector3 startPos = new Vector3(0, 0, 0);
    private Vector3 location;
    private GameObject homingTarget;

    // Start is called before the first frame update
    void Start()
    {
        GetWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (homingTarget)
        {
            if (gameObject.transform.position != homingTarget.transform.position)
            {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, homingTarget.transform.position, (flightSpeed * 1.5f * Time.deltaTime));
            }
            else
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                AIController AI_temp;
                CarController p_temp;
                if (AI_temp = homingTarget.gameObject.GetComponent<AIController>())
                {
                    AI_temp.GoFlying();
                }
                if (p_temp = homingTarget.gameObject.GetComponent<CarController>())
                {
                    p_temp.GoFlying();
                }
            }
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                fired = false;
                currentTime = 0;
                gameObject.SetActive(false);
            }
            return;
        }
        Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
        if (gameObject.transform.position != waypoints[currentWaypoint].position)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, waypoints[currentWaypoint].position, (flightSpeed * Time.deltaTime));
        }
        transform.LookAt(waypoints[currentWaypoint]);

        if (RelativeWaypointPosition.magnitude < 1)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }
    }

    public override void Fire()
    {

        LaunchButt(target);
        fired = true;
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

    public void LaunchButt(GameObject targetCar)
    {
        location = targetCar.transform.position;
        location.y = location.y + 7;
        transform.position = location;
        if (AIScript = targetCar.gameObject.GetComponent<AIController>())
        {
            currentWaypoint = AIScript.GetCurrentWaypointInt();
            AIScript.currentPowerup = null;

        }
        if (CarScript = targetCar.gameObject.GetComponent<CarController>())
        {
            currentWaypoint = CarScript.GetCurrentWaypointInt();
            CarScript.currentPowerup = null;
        }
    }

    public void ResetRocket()
    {
        transform.position = startPos;
        fired = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car") && other.gameObject != target.gameObject)
        {
            homingTarget = other.gameObject;
        }
    }
}
