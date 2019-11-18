using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public int thisCarNum;

    public Transform WheelMesh_FrontLeft;
    public Transform WheelMesh_FrontRight;
    public Transform WheelMesh_BackLeft;
    public Transform WheelMesh_BackRight;
    public WheelCollider WheelCollider_FrontLeft;
    public WheelCollider WheelCollider_FrontRight;
    public WheelCollider WheelCollider_BackLeft;
    public WheelCollider WheelCollider_BackRight;
    public GameObject ButtShieldMesh;

    public float maxTurnAngle = 10;
    public float maxTorque = 10;
    public float topSpeed;
    public float maxReverseSpeed = -50;

    public float decelerationTorque = 30;
    public float maxBrakeTorque = 100;
    public float handBrakeFSlip = 0.04f;
    public float handBrakeSSlip = 0.04f;
    public float brakingDistance = 6f;
    public float forwardOffset;

    public Transform WaypointHolder;

    private Rigidbody body;
    private Vector3 centerOfMassAdjustment = new Vector3(0f, -0.9f, 0f);
    private float spoilerRatio = 1.5f;
    private float currentSpeed;
    private float inputSteer;
    private float inputTorque;
    private bool applyHandBrake = false;
    private Transform[] waypoints;
    private int currentWaypoint = 0;

    public GameObject currentPowerup;
    private PickupBox pickup;
    private bool flying = false;
    private float flyingSpeedRatio = 700f;
    private bool useAbility = false;
    private int deathTimer = 0;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassAdjustment;

        //Get the waypoints from the track
        GetWaypoints();
    }

    private void Update()
    {
        if (RaceManager.Instance.raceStarted)
        {
            UpdateWheelPositions();
            // Keeping your thicc butt attached to your car
            ButtShieldMesh.transform.position = gameObject.transform.position;
            ButtShieldMesh.transform.rotation = gameObject.transform.rotation;
        }
    }

    private void FixedUpdate()
    {
        if (RaceManager.Instance.raceStarted)
        {
            // Waypoint checking
            Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
            inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;
            if (RelativeWaypointPosition.magnitude < 15)
            {
                currentWaypoint++;
                if (currentWaypoint >= waypoints.Length)
                {
                    currentWaypoint = 0;
                    RaceManager.Instance.LapFinishedByAI(this);
                }
            }

            WheelCollider_FrontLeft.steerAngle = inputSteer * maxTurnAngle;
            WheelCollider_FrontRight.steerAngle = inputSteer * maxTurnAngle;

            //spoiler
            Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
            body.AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Impulse);

            if (Mathf.Abs(inputSteer) < 0.5f)
            {
                inputTorque = RelativeWaypointPosition.z / RelativeWaypointPosition.magnitude;
                applyHandBrake = false;
            }
            else
            {
                if (body.velocity.magnitude > 10)
                {
                    applyHandBrake = true;
                }
                else if (localVelocity.z > 10)
                {
                    applyHandBrake = false;
                    inputTorque = -1;
                    inputSteer *= -1;
                }
                else
                {
                    applyHandBrake = false;
                    inputTorque = 0;
                }
            }

            //Hand BRAKE
            if (applyHandBrake)
            {
                SetSlipValues(handBrakeFSlip, handBrakeSSlip);
            }
            else
            {
                SetSlipValues(1f, 1f);
            }


            // KM/H
            currentSpeed = WheelCollider_BackLeft.radius * WheelCollider_BackLeft.rpm * Mathf.PI * 0.12f;
            if (currentSpeed < topSpeed && currentSpeed > maxReverseSpeed)
            {
                float adjustment = ForwardRaycast();
                //rear wheel drive
                WheelCollider_BackRight.motorTorque = adjustment * inputTorque * maxTorque;
                WheelCollider_BackLeft.motorTorque = adjustment * inputTorque * maxTorque;
            }
            else
            {
                WheelCollider_BackLeft.motorTorque = 0;
                WheelCollider_BackRight.motorTorque = 0;
            }

            if (useAbility)
            {
                SpeedBoost activePowerup_Speed;
                ButtShield activePowerup_Shield;
                if (currentPowerup != null)
                {
                    if (activePowerup_Speed = currentPowerup.gameObject.GetComponent<SpeedBoost>())
                    {
                        activePowerup_Speed.SetTarget(this.gameObject);
                        activePowerup_Speed.Fire();
                    }
                    if (activePowerup_Shield = currentPowerup.gameObject.GetComponent<ButtShield>())
                    {
                        activePowerup_Shield.SetTarget(this.gameObject);
                        activePowerup_Shield.Fire();
                    }
                    useAbility = false;
                }
            }

            // Respawning after hitting rocket or big butt with delay
            if (flying)
            {
                deathTimer++;
                if (deathTimer >= 200)
                {
                    int i = thisCarNum;
                    RaceManager.Instance.AIRespawn(RaceManager.Instance.lastWaypoint[i], RaceManager.Instance.nextWaypoint[i], i);
                    deathTimer = 0;
                    flying = false;
                }
            }
        }
    }


    private void SetSlipValues(float F, float S)
    {
        WheelFrictionCurve tempStruct = WheelCollider_BackRight.forwardFriction;
        tempStruct.stiffness = F;
        WheelCollider_BackRight.forwardFriction = tempStruct;

        tempStruct = WheelCollider_BackLeft.forwardFriction;
        tempStruct.stiffness = F;
        WheelCollider_BackLeft.forwardFriction = tempStruct;

        tempStruct = WheelCollider_BackRight.sidewaysFriction;
        tempStruct.stiffness = S;
        WheelCollider_BackRight.sidewaysFriction = tempStruct;

        tempStruct = WheelCollider_BackLeft.sidewaysFriction;
        tempStruct.stiffness = S;
        WheelCollider_BackLeft.sidewaysFriction = tempStruct;

    }

    void UpdateWheelPositions()
    {
        float rotationThisFrame = -360 * Time.deltaTime;
        WheelMesh_FrontRight.transform.GetChild(0).Rotate(0, WheelCollider_BackRight.rpm / rotationThisFrame, 0);
        WheelMesh_BackRight.transform.GetChild(0).Rotate(0, WheelCollider_BackRight.rpm / rotationThisFrame, 0);
        WheelMesh_FrontLeft.transform.GetChild(0).Rotate(0, WheelCollider_BackLeft.rpm / rotationThisFrame, 0);
        WheelMesh_BackLeft.transform.GetChild(0).Rotate(0, WheelCollider_BackLeft.rpm / rotationThisFrame, 0);

        float tempY = WheelCollider_FrontLeft.steerAngle + gameObject.transform.eulerAngles.y;
        Vector3 wheelRotation = new Vector3(WheelMesh_FrontLeft.rotation.x, tempY, WheelMesh_FrontLeft.rotation.z + 90);
        Quaternion rotation = Quaternion.Euler(wheelRotation);
        WheelMesh_FrontLeft.rotation = rotation;
        WheelMesh_FrontRight.rotation = rotation;
    }

    public void GetWaypoints()
    {
        Transform[] potentialWaypoints = WaypointHolder.GetComponentsInChildren<Transform>();
        waypoints = new Transform[(potentialWaypoints.Length - 1)];

        for (int i = 1; i < potentialWaypoints.Length; i++)
        {
            waypoints[i - 1] = potentialWaypoints[i];
        }
    }

    public Transform GetCurrentWaypoint()
    {
        return waypoints[currentWaypoint];
    }

    public int GetCurrentWaypointInt()
    {
        return currentWaypoint;
    }
    public Transform GetLastWaypoint()
    {
        if (currentWaypoint - 1 < 0)
        {
            return waypoints[waypoints.Length - 1];
        }
        return waypoints[currentWaypoint - 1];
    }

    float ForwardRaycast()
    {
        RaycastHit hit;
        Vector3 CarFront = transform.position + (transform.forward * forwardOffset);
        Debug.DrawRay(CarFront, transform.forward * brakingDistance);

        if (Physics.Raycast(CarFront, transform.forward, out hit, brakingDistance))
        {
            return (((CarFront - hit.point).magnitude / brakingDistance) * 2) * -1;
        }

        return 1f;
    }

    public void SetCurrentPowerup(GameObject newPowerup)
    {
        currentPowerup = newPowerup;
        useAbility = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentPowerup == null)
        {
            if (pickup = other.gameObject.GetComponent<PickupBox>())
            {
                Debug.Log("GotPowerup");
                pickup.Despawn();
                RaceManager.Instance.NewPowerup(this);
            }
        }
        if (other.gameObject.CompareTag("ButtShield") &&
            other.gameObject != gameObject.transform.GetChild(1).transform.GetChild(0).gameObject &&
            other.gameObject != gameObject.transform.GetChild(1).transform.GetChild(1).gameObject)
        {
            GoFlying();
        }
    }

    private void GoFlying()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
        body.AddForce(transform.up * (localVelocity.z * flyingSpeedRatio), ForceMode.Impulse);
        flying = true;
    }
}
