using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public Rigidbody[] cars;
    public float respawnDelay = 5f;
    public float distanceToCover = 1f;
    private AIController[] scripts;
    private float[] respawnTimes;
    private float[] distanceLeftToTravel;
    private Transform[] waypoint;

    private int[] laps;

    public Texture2D GO;
    public Texture2D three;
    public Texture2D two;
    public Texture2D one;
    private int countdownTimerDelay;
    private float countdownTimerStartTime;

    public static RaceManager Instance { get { return instance; } }
    private static RaceManager instance = null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        CountDownTimerReset(3);
    }



    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(CountDownTimerImage());

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    Texture2D CountDownTimerImage()
    {
        switch (CountdownTimerSecondsRemaining())
        {
            case 3:
                return three;
            case 2:
                return two;
            case 1:
                return one;
            case 0:
                return GO;
            default:
                return null;
        }
    }

    int CountdownTimerSecondsRemaining()
    {
        int elapsedSeconds = (int)(Time.time - countdownTimerStartTime);
        int secondsLeft = (countdownTimerDelay - elapsedSeconds);
        return secondsLeft;
    }

    void CountDownTimerReset(int delayInSeconds)
    {
        countdownTimerDelay = delayInSeconds;
        countdownTimerStartTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        respawnTimes = new float[cars.Length];
        distanceLeftToTravel = new float[cars.Length];
        scripts = new AIController[cars.Length];
        waypoint = new Transform[cars.Length];
        laps = new int[cars.Length];

        //initialize the arrays with starting values
        for (int i = 0; i < respawnTimes.Length; i++)
        {
            scripts[i] = cars[i].gameObject.GetComponent<AIController>();
            respawnTimes[i] = respawnDelay;
            distanceLeftToTravel[i] = float.MaxValue;
            laps[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cars.Length; i++)
        {
            Transform nextWaypoint = scripts[i].GetCurrentWaypoint();
            float distanceCovered = (nextWaypoint.position - cars[i].position).magnitude;

            if (distanceLeftToTravel[i] - distanceToCover > distanceCovered ||
                waypoint[i] != nextWaypoint)
            {
                waypoint[i] = nextWaypoint;
                respawnTimes[i] = respawnDelay;
                distanceLeftToTravel[i] = distanceCovered;
            }
            else
            {
                respawnTimes[i] -= Time.deltaTime;
            }
            if (respawnTimes[i] <= 0)
            {
                respawnTimes[i] = respawnDelay;
                distanceLeftToTravel[i] = float.MaxValue;
                cars[i].velocity = Vector3.zero;

                Transform lastWaypoint = scripts[i].GetLastWaypoint();
                cars[i].position = lastWaypoint.position;
                cars[i].rotation = Quaternion.LookRotation(nextWaypoint.position - lastWaypoint.position);
            }
            if (laps[i] >= 3)
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    public void LapFinishedByAI(AIController script)
    {
        for (int i = 0; i < respawnTimes.Length; i++)
        {
            if (scripts[i] == script)
            {
                laps[i]++;
                break;
            }
        }
    }
}
