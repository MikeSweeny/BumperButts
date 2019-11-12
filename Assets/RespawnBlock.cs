using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBlock : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<AIComponent>())
        {
            int thisCarNum = other.gameObject.GetComponent<AIComponent>().thisCarNum;
            RaceManager.Instance.AIRespawn(RaceManager.Instance.lastWaypoint[thisCarNum], RaceManager.Instance.nextWaypoint[thisCarNum], thisCarNum);
        }
        else if (other.gameObject.GetComponent<PlayerComponent>())
        {
            RaceManager.Instance.PlayerRespawn(RaceManager.Instance.p_lastWaypoint, RaceManager.Instance.p_nextWaypoint);
        }
        
    }

}
