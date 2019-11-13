using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public bool fired = false;
    public bool triggered = false;
    protected GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void Fire()
    {

    }

    public void SetTarget(GameObject targetCar)
    {
        target = targetCar;
    }
}
