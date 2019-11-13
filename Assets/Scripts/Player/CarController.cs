using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform WheelMesh_FrontLeft;
    public Transform WheelMesh_FrontRight;
    public Transform WheelMesh_BackLeft;
    public Transform WheelMesh_BackRight;
    public WheelCollider WheelCollider_FrontLeft;
    public WheelCollider WheelCollider_FrontRight;
    public WheelCollider WheelCollider_BackLeft;
    public WheelCollider WheelCollider_BackRight;

    public float maxTurnAngle = 20f;
    public float maxTorque = 100f;
    public float topSpeed = 200f;

    public float maxReverseSpeed = -100f;
    public float decelerationTorque = 3f;
    public float maxBrakeTorque = 4f;
    public float handBrakeForwardSlip = 0.08f;
    public float handBrakeSidewaysSlip = 0.04f;

    public float brakingDistance = 1f;
    public float forwardOffset;
    public Transform WaypointHolder;

    private float currentSpeed;
    private Vector3 centerOfMassOffset = new Vector3(0f, -0.9f, 0f);
    private Rigidbody body;
    private float spoilerRatio = 2f;
    private bool applyHandBrake = false;
    private Transform[] waypoints;
    private int currentWaypoint = 0;
    private float inputSteer;
    private float inputTorque;

    public Powerup currentPowerup;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassOffset;

        //Get the waypoints from the track
        GetWaypoints();
    }

    // Update is called once per frame
    void Update()
    { 
        UpdateWheelPositions();
    }

    private void FixedUpdate()
    {
        //front wheel steering
        WheelCollider_FrontRight.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
        WheelCollider_FrontLeft.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
        
        //spoiler
        Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
        body.AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Impulse);

        //braking
        if (!applyHandBrake && (Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0 || (Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)))
        {
            WheelCollider_BackLeft.brakeTorque = decelerationTorque + maxTorque;
            WheelCollider_BackRight.brakeTorque = decelerationTorque + maxTorque;
        }
        else if (!applyHandBrake && (Input.GetAxis("Vertical") == 0))
        {
            WheelCollider_BackLeft.brakeTorque = decelerationTorque;
            WheelCollider_BackRight.brakeTorque = decelerationTorque;
        }
        else
        {
            WheelCollider_BackLeft.brakeTorque = 0;
            WheelCollider_BackRight.brakeTorque = 0;
        }

        // KM/H
        currentSpeed = WheelCollider_BackLeft.radius * WheelCollider_BackRight.rpm * Mathf.PI * 0.12f;
        if (currentSpeed < topSpeed && currentSpeed > maxReverseSpeed)
        {
            //rear wheel drive
            WheelCollider_BackRight.motorTorque = Input.GetAxis("Vertical") * maxTorque;
            WheelCollider_BackLeft.motorTorque = Input.GetAxis("Vertical") * maxTorque;
        }
        else
        {
            WheelCollider_BackLeft.motorTorque = 0;
            WheelCollider_BackRight.motorTorque = 0;
        }

        //Hand BRAKE
        if (Input.GetButton("Jump"))
        {
            applyHandBrake = true;
            WheelCollider_FrontLeft.brakeTorque = maxBrakeTorque;
            WheelCollider_FrontRight.brakeTorque = maxBrakeTorque;

            //Powerslide
            if (GetComponent<Rigidbody>().velocity.magnitude > 1)
            {
                SetSlipValues(handBrakeForwardSlip, handBrakeSidewaysSlip);
            }
            else
            {
                SetSlipValues(1f, 1f);
            }
        }
        else
        {
            applyHandBrake = false;
            WheelCollider_FrontLeft.brakeTorque = 0;
            WheelCollider_FrontRight.brakeTorque = 0;
            SetSlipValues(1f, 1f);
        }
        // Waypoint tracking
        Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x, transform.position.y, waypoints[currentWaypoint].position.z));
        if (RelativeWaypointPosition.magnitude < 25)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
                RaceManager.Instance.LapFinishedByPlayer(this);
            }
        }

        // Using Powerup
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentPowerup.SetTarget(this.gameObject);
            currentPowerup.triggered = true;
        }
    }

    // Handbrake Slip
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

    // Making the wheel meshes respond to movement
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

    // Waypoint related functions
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

    public Transform GetLastWaypoint()
    {
        if (currentWaypoint - 1 < 0)
        {
            return waypoints[waypoints.Length - 1];
        }
        return waypoints[currentWaypoint - 1];
    }
}
