using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : Powerup
{
    private float duration = 3;
    private float speedMultiplier = 3f;
    private float currentTime;


    private AIController AIScript;
    private CarController CarScript;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (triggered)
        {
            Fire();
            triggered = false;
        }
        if (fired)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                ResetSpeed(target);
                fired = false;
                currentTime = 0;
            }
        }

    }

    private void ResetSpeed(GameObject targetCar)
    {
        if (AIScript = targetCar.gameObject.GetComponent<AIController>())
        {
            AIScript.topSpeed /= speedMultiplier / 1.5f;
            AIScript.maxTorque /= speedMultiplier;
            AIScript.decelerationTorque /= speedMultiplier;
        }
        if (CarScript = targetCar.gameObject.GetComponent<CarController>())
        {
            CarScript.topSpeed /= speedMultiplier / 1.5f;
            CarScript.maxTorque /= speedMultiplier;
            CarScript.decelerationTorque /= speedMultiplier;
        }
    }

    protected override void Fire()
    {
        base.Fire();
        GiveSpeedBoost(target);
        fired = true;
    }

    private void GiveSpeedBoost(GameObject targetCar)
    {
        if (AIScript = targetCar.gameObject.GetComponent<AIController>())
        {
            AIScript.topSpeed *= speedMultiplier / 1.5f;
            AIScript.maxTorque *= speedMultiplier;
            AIScript.decelerationTorque *= speedMultiplier;
        }
        if (CarScript = targetCar.gameObject.GetComponent<CarController>())
        {
            CarScript.topSpeed *= speedMultiplier / 1.5f;
            CarScript.maxTorque *= speedMultiplier;
            CarScript.decelerationTorque *= speedMultiplier;
        }
    }
}
