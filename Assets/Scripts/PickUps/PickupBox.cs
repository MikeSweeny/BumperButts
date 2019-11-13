using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBox : MonoBehaviour
{
    public bool pickedUp = false;
    private float respawnTime = 10f;
    private float timer = 0;

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

        // Respawning
        if (pickedUp == true)
        {
            if (timer < respawnTime)
            {
                timer += Time.deltaTime;
            }
            else
            {
                Respawn();
                timer = 0;
            }
        }
    }

    public void Respawn()
    {
        gameObject.GetComponent<SphereCollider>().enabled = true;
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
        pickedUp = false;
    }

    public void Despawn()
    {
        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }

}
