using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtRocket : Powerup
{
    private float maxSpeed;
    private Transform[] waypoints;
    public Transform waypointHolder;
    public int currentWaypoint = 0;
    private float flightSpeed = 40f;

    public Vector3 startPos = new Vector3(0, 0, 0);
    private Vector3 location;
    private GameObject homingTarget;

    // Start is called before the first frame update
    void Start()
    {
        duration = 0.5f;
        GetWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (homingTarget)
        {
            fired = false;
            if (gameObject.transform.position != homingTarget.transform.position)
            {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, homingTarget.transform.position, (flightSpeed * 1.5f * Time.deltaTime));
                gameObject.transform.LookAt(homingTarget.transform.position);
            }
            else
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                gameObject.transform.GetChild(0).gameObject.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                gameObject.transform.GetChild(0).gameObject.transform.GetChild(3).GetComponent<ParticleSystem>().Play();

                gameObject.transform.GetChild(1).gameObject.SetActive(false);
                gameObject.transform.GetChild(2).gameObject.SetActive(false);
                gameObject.transform.GetChild(3).gameObject.SetActive(false);
                gameObject.transform.GetChild(4).gameObject.SetActive(false);
                AIController AI_temp;
                CarController p_temp;
                if (AI_temp = homingTarget.gameObject.transform.parent.gameObject.transform.parent.GetComponent<AIController>())
                {
                    AI_temp.GoFlying();
                }
                if (p_temp = homingTarget.gameObject.transform.parent.gameObject.transform.parent.GetComponent<CarController>())
                {
                    p_temp.GoFlying();
                }
            }
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                currentTime = 0;
                ResetRocket();
            }
        }
    }

    private void FixedUpdate()
    {
        if (fired)
        {
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
        homingTarget = null;
        transform.position = startPos;
        fired = false;

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
        gameObject.transform.GetChild(3).gameObject.SetActive(true);
        gameObject.transform.GetChild(4).gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car") && other.gameObject.transform.parent.gameObject.transform.parent.gameObject != target.gameObject)
        {
            homingTarget = other.gameObject;
        }
    }
}
