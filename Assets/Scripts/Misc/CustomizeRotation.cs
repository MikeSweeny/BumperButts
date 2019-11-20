using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeRotation : MonoBehaviour
{
    float rotSpeed = 10f;
    Quaternion currentRot;
    Rigidbody body;
    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        currentRot = body.transform.rotation;
        float tempY = currentRot.eulerAngles.y + (rotSpeed * Time.deltaTime);
        Vector3 tempRot = new Vector3(currentRot.eulerAngles.x, tempY, currentRot.eulerAngles.z);
        Quaternion newRot = Quaternion.Euler(tempRot);
        body.transform.rotation = newRot;
    }
}
