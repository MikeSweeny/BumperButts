using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public bool fired = false;
    protected GameObject target;

    protected AIController AIScript;
    protected CarController CarScript;

    protected float duration;
    protected float currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Fire()
    {

    }

    public void SetTarget(GameObject targetCar)
    {
        gameObject.SetActive(true);
        target = targetCar;
    }
}
