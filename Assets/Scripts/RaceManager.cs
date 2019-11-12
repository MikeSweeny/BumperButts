using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public Rigidbody[] cars;
    public Rigidbody p_car;

    public float respawnDelay = 5f;
    public float p_respawnDelay = 5f;

    public float distanceToCover = 1f;
    public float p_distanceToCover = 1f;

    private AIController[] scripts;
    private CarController p_script;

    private float[] respawnTimes;
    private float p_respawnTime;

    private float[] distanceLeftToTravel;
    private float p_distanceLeftToTravel;

    private Transform[] waypoint;
    private Transform p_waypoint;

    public int[] laps;
    public int p_laps;

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
        p_script = p_car.GetComponent<CarController>();
        p_respawnTime = p_respawnDelay;
        p_distanceLeftToTravel = float.MaxValue;
        p_laps = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // AI lap navigate logic
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

                if (laps[i] >= 3)
                {
                    SceneManager.LoadScene("RaceLevel");
                }
            }
        }
        // Player lap navigate logic
        Transform p_nextWaypoint = p_script.GetCurrentWaypoint();
        float p_distanceCovered = (p_nextWaypoint.position - p_car.position).magnitude;
        if (p_distanceCovered - p_distanceToCover > p_distanceCovered ||
            p_waypoint != p_nextWaypoint)
        {
            p_waypoint = p_nextWaypoint;
            p_respawnTime = p_respawnDelay;
            p_distanceLeftToTravel = p_distanceCovered;
        }
        else
        {
            p_respawnTime -= Time.deltaTime;
        }
        if (p_respawnTime <= 0)
        {
            p_respawnTime = p_respawnDelay;
            p_distanceLeftToTravel = float.MaxValue;
            p_car.velocity = Vector3.zero;

            Transform p_lastWaypoint = p_script.GetLastWaypoint();
            p_car.position = p_lastWaypoint.position;
            p_car.rotation = Quaternion.LookRotation(p_nextWaypoint.position - p_lastWaypoint.position);

            if (p_laps >= 3)
            {
                SceneManager.LoadScene("RaceLevel");
            }
        }
    }

    public void LapFinishedByPlayer(CarController script)
    {
        p_laps++;
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
