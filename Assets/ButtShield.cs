using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtShield : Powerup
{

    // Start is called before the first frame update
    void Start()
    {
        duration = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        if (fired)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                ResetButt(target);
                fired = false;
                currentTime = 0;
            }
        }
    }

    public override void Fire()
    {
        GiveBigButt(target);
        fired = true;
    }

    public void ResetButt(GameObject targetCar)
    {
        targetCar.transform.GetChild(1).gameObject.SetActive(false);
        if (AIScript = targetCar.gameObject.GetComponent<AIController>())
        {
            //AIScript.currentPowerup = null;
        }
        if (CarScript = targetCar.gameObject.GetComponent<CarController>())
        {
            CarScript.currentPowerup = null;
        }
    }

    public void GiveBigButt(GameObject targetCar)
    {
        targetCar.transform.GetChild(1).gameObject.SetActive(true);
    }
}
