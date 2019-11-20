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
    private AIComponent[] components;
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

    // Making these persistent for the lava respawner
    public Transform[] nextWaypoint;
    public Transform[] lastWaypoint;
    public int currentCar = 0;

    public Transform p_nextWaypoint;
    public Transform p_lastWaypoint;

    public List<PickupBox> allPickups;
    private enum powerupTypes { Speed, Shield, Rocket};
    private GameObject activePowerup;

    public GameObject speedPrefab;
    private GameObject p_speedPrefab;
    private GameObject[] speedPrefabs;

    public GameObject shieldPrefab;
    private GameObject p_shieldPrefab;
    private GameObject[] shieldPrefabs;

    public GameObject rocketPrefab;
    private GameObject p_rocketPrefab;
    private GameObject[] rocketPrefabs;

    public bool raceStarted;
    public bool m_isPaused = false;
    public GameObject m_pauseMenu;
    public GameObject m_startMenu;
    public GameObject m_winMenu;
    public GameObject m_loseMenu;

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

        raceStarted = false;
        m_isPaused = true;
        m_startMenu.SetActive(true);
        Time.timeScale = 0;
        CountDownTimerReset(4);
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
            case 4:
                return null;
            case 3:
                return three;
            case 2:
                return two;
            case 1:
                return one;
            case 0:
                raceStarted = true;
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
        components = new AIComponent[cars.Length];
        lastWaypoint = new Transform[cars.Length];
        nextWaypoint = new Transform[cars.Length];
        waypoint = new Transform[cars.Length];
        laps = new int[cars.Length];
        speedPrefabs = new GameObject[cars.Length];
        shieldPrefabs = new GameObject[cars.Length];
        rocketPrefabs = new GameObject[cars.Length];
        PickupBox[] boxes = (PickupBox[])GameObject.FindObjectsOfType<PickupBox>();
        foreach (PickupBox pickup in boxes)
        {
            allPickups.Add(pickup);
        }

        //initialize the arrays with starting values
        for (int i = 0; i < respawnTimes.Length; i++)
        {
            scripts[i] = cars[i].gameObject.GetComponent<AIController>();
            scripts[i].thisCarNum = i;
            respawnTimes[i] = respawnDelay;
            distanceLeftToTravel[i] = float.MaxValue;
            laps[i] = 0;
            speedPrefabs[i] = Instantiate(speedPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            shieldPrefabs[i] = Instantiate(shieldPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            rocketPrefabs[i] = Instantiate(rocketPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            rocketPrefabs[i].gameObject.GetComponent<ButtRocket>().waypointHolder = GameObject.FindWithTag("WaypointHolder").transform;
        }
        p_script = p_car.GetComponent<CarController>();
        p_respawnTime = p_respawnDelay;
        p_distanceLeftToTravel = float.MaxValue;
        p_laps = 0;

        p_speedPrefab = Instantiate(speedPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        p_shieldPrefab = Instantiate(shieldPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        p_rocketPrefab = Instantiate(rocketPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        p_rocketPrefab.gameObject.GetComponent<ButtRocket>().waypointHolder = GameObject.FindWithTag("WaypointHolder").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!raceStarted)
        {
            return;
        }
        // AI lap navigate logic
        for (int i = 0; i < cars.Length; i++)
        {
            nextWaypoint[i] = scripts[i].GetCurrentWaypoint();
            lastWaypoint[i] = scripts[i].GetLastWaypoint();
            float distanceCovered = (nextWaypoint[i].position - cars[i].position).magnitude;

            if (distanceLeftToTravel[i] - distanceToCover > distanceCovered ||
                waypoint[i] != nextWaypoint[i])
            {
                waypoint[i] = nextWaypoint[i];
                respawnTimes[i] = respawnDelay;
                distanceLeftToTravel[i] = distanceCovered;
            }
            else
            {
                respawnTimes[i] -= Time.deltaTime;
            }
            if (respawnTimes[i] <= 0)
            {
                AIRespawn(lastWaypoint[i], nextWaypoint[i], i);
            }
            if (laps[i] == 3 && scripts[i].GetCurrentWaypointInt() == 2)
            {
                raceStarted = false;
                PlayerLost();
            }
        }
        // Player lap navigate logic
        p_nextWaypoint = p_script.GetCurrentWaypoint();
        p_lastWaypoint = p_script.GetLastWaypoint();
        float p_distanceCovered = (p_nextWaypoint.position - p_car.position).magnitude;
        if (p_distanceLeftToTravel - p_distanceToCover > p_distanceCovered ||
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
            PlayerRespawn(p_lastWaypoint, p_nextWaypoint);
        }
        if (p_laps >= 3 && p_script.GetCurrentWaypointInt() == 2)
        {
            raceStarted = false;
            PlayerWin();
        }


    }
    public void PlayerRespawn(Transform p_last, Transform p_next)
    {
        p_respawnTime = p_respawnDelay;
        p_distanceLeftToTravel = float.MaxValue;
        p_car.velocity = Vector3.zero;
        p_car.position = p_last.position;
        p_car.rotation = Quaternion.LookRotation(p_next.position - p_last.position);
    }

    public void AIRespawn(Transform last, Transform next, int i)
    {
        respawnTimes[i] = respawnDelay;
        distanceLeftToTravel[i] = float.MaxValue;
        cars[i].velocity = Vector3.zero;
        cars[i].position = last.position;
        cars[i].rotation = Quaternion.LookRotation(next.position - last.position);
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

    public void NewPowerup(CarController script)
    {
        //int rand = Random.Range(1, 4);
        int rand = 3;
        if (rand == 1)
        {
        activePowerup = p_shieldPrefab;
        }
        if (rand == 2)
        {
            activePowerup = p_speedPrefab;
        }
        if (rand == 3)
        {
            activePowerup = p_rocketPrefab;
        }
        script.SetCurrentPowerup(activePowerup);
    }

    public void NewPowerup(AIController script)
    {
        int rand = Random.Range(1, 4);
        if (rand == 1)
        {
            activePowerup = shieldPrefabs[script.thisCarNum];
        }
        if (rand == 2)
        {
            activePowerup = speedPrefabs[script.thisCarNum];
        }
        if (rand == 3)
        {
            activePowerup = rocketPrefabs[script.thisCarNum];
        }
        script.SetCurrentPowerup(activePowerup);
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        m_isPaused = false;
        m_startMenu.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        Debug.Log("Pause Toggled");
        if (m_isPaused)
        {
            UnPauseGame();
        }
        else if (!m_isPaused)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        m_isPaused = true;
        m_pauseMenu.SetActive(true);
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        m_isPaused = false;
        m_pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("RaceLevel");
    }

    public void PlayerWin()
    {
        raceStarted = false;
        m_winMenu.SetActive(true);
    }

    public void PlayerLost()
    {
        Time.timeScale = 0;
        m_isPaused = true;
        m_loseMenu.SetActive(true);
    }
}
