using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : Powerup
{
    private float speedMultiplier = 3f;

    // Start is called before the first frame update
    void Start()
    {
        duration = 3f;
    }

    // Update is called once per frame
    void Update()
    {
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

    public override void Fire()
    {
        GiveSpeedBoost(target);
        fired = true;
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

    private void GiveSpeedBoost(GameObject targetCar)
    {
        if (AIScript = targetCar.gameObject.GetComponent<AIController>())
        {
            AIScript.topSpeed *= speedMultiplier / 1.5f;
            AIScript.maxTorque *= speedMultiplier;
            AIScript.decelerationTorque *= speedMultiplier;

            AIScript.currentPowerup = null;
        }
        if (CarScript = targetCar.gameObject.GetComponent<CarController>())
        {
            CarScript.topSpeed *= speedMultiplier / 1.5f;
            CarScript.maxTorque *= speedMultiplier;
            CarScript.decelerationTorque *= speedMultiplier;

            CarScript.currentPowerup = null;
        }
    }
}
