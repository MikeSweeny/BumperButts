using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform car;
    public float rotationDamping = 3f;
    public float heightDamping = 2f;
    private float desiredAngle = 0;
    private float distance = 8;
    private float desiredDistance;
    private float height = 3.75f;
    private bool cameraReset = false;

    private void Start()
    {
        desiredDistance = distance;
    }

    private void FixedUpdate()
    {
        float currentAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        desiredAngle = car.eulerAngles.y;

        // if backing up
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 localVelocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
            if (localVelocity.z < -0.5f)
            {
                desiredAngle += 180;
            }
        }


        float desiredHeight = car.position.y + height;

        currentAngle = Mathf.LerpAngle(currentAngle, desiredAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, desiredHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);

        Vector3 finalPosition = car.position - (currentRotation * Vector3.forward * distance);
        finalPosition.y = currentHeight;
        transform.position = finalPosition;
        transform.LookAt(car);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "ButtRocket")
        {
            distance -= 2f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "ButtRocket")
        {
            CameraSlowReset();
        }
    }
    private void Update()
    {
        if (distance <= desiredDistance && cameraReset)
        {
            distance += 0.01f;
            return;
        }
        cameraReset = false;
    }

    private void CameraSlowReset()
    {
        cameraReset = true;
    }
}


