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

    public Transform WaypointHolder;
    public float brakingDistance = 1f;
    public float forwardOffset;

    private float currentSpeed;
    private Vector3 centerOfMassOffset = new Vector3(0f, -0.9f, 0f);
    private Rigidbody body;
    private float spoilerRatio = 0.2f;
    private bool applyHandBrake = false;
    private Transform[] waypoints;
    private int currentWaypoint = 0;
    private float inputSteer;
    private float inputTorque;



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
        float rotationThisFrame = -360 * Time.deltaTime;
        WheelMesh_FrontRight.Rotate(0, WheelCollider_FrontRight.rpm / rotationThisFrame, 0);
        WheelMesh_BackRight.Rotate(0, WheelCollider_BackRight.rpm / rotationThisFrame, 0);
        WheelMesh_FrontLeft.Rotate(0, WheelCollider_FrontLeft.rpm / rotationThisFrame, 0);
        WheelMesh_BackLeft.Rotate(0, WheelCollider_BackLeft.rpm / rotationThisFrame, 0);

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

    // Spinning the wheel meshes with the collider's speed
    void UpdateWheelPositions()
    {
        WheelHit Contact = new WheelHit();

        if (WheelCollider_FrontLeft.GetGroundHit(out Contact))
        {
            Vector3 temp = WheelCollider_FrontLeft.transform.position;
            temp.y = (Contact.point + (WheelCollider_FrontLeft.transform.up * WheelCollider_FrontLeft.radius)).y;
            WheelMesh_FrontLeft.position = temp;

            temp = WheelCollider_FrontRight.transform.position;
            temp.y = (Contact.point + (WheelCollider_FrontRight.transform.up * WheelCollider_FrontRight.radius)).y;
            WheelMesh_FrontRight.position = temp;

            temp = WheelCollider_BackLeft.transform.position;
            temp.y = (Contact.point + (WheelCollider_BackLeft.transform.up * WheelCollider_BackLeft.radius)).y;
            WheelMesh_BackLeft.position = temp;

            temp = WheelCollider_BackRight.transform.position;
            temp.y = (Contact.point + (WheelCollider_BackRight.transform.up * WheelCollider_BackRight.radius)).y;
            WheelMesh_BackRight.position = temp;
        }
    }

    // Checking for cars infront
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
