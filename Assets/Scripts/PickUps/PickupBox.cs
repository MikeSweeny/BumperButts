using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBox : MonoBehaviour
{
    public bool pickedUp = false;

    Vector3 startPos;
    Vector3 currentPos;

    Quaternion currentRot;
    bool bobUp = false;
    Rigidbody body;

    float spinSpeed = 0.4f;
    float bobSpeed = 0.001f;
    float bobHeight = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.transform.GetChild(0).GetComponent<Rigidbody>();
        startPos = body.transform.position;
        currentPos = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        // Spinning
        currentRot = body.transform.rotation;
        float tempY = currentRot.eulerAngles.y + spinSpeed;
        Vector3 tempRot = new Vector3(currentRot.eulerAngles.x, tempY, currentRot.eulerAngles.z);
        Quaternion newRot = Quaternion.Euler(tempRot);
        body.transform.rotation = newRot;

        // Bobbing
        if (currentPos.y >= (startPos.y - bobHeight) &&
            bobUp == false)
        {
            currentPos.y -= bobSpeed;
            body.position = currentPos;
        }
        else
        {
            bobUp = true;
        }
        if (currentPos.y <= (startPos.y + bobHeight) &&
            bobUp == true)
        {
            currentPos.y += bobSpeed;
            body.position = currentPos;
        }
        else
        {
            bobUp = false;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerComponent>())
        {
            pickedUp = true;
            Debug.Log("Picked Up");
        }
    }
}
