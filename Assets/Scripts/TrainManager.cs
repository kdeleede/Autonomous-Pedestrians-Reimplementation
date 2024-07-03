using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{

    public static TrainManager instance;
    public List<Train> trains = new List<Train>();
    private float departureInterval = 2f * 60f; // 2 minutes in seconds
    private float maxDepartureTime = 10 * 60f; // 10 minutes in seconds

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        ScheduleInitialDepartures();
    }

    private void ScheduleInitialDepartures()
    {
        for (int i = 0; i < trains.Count; i++)
        {
            trains[i].timeTillDeparture = departureInterval * (i + 1);
        }
    }

    public float GetNextDepartureTime()
    {
        return maxDepartureTime;
    }

    public Train assignTrain()
    {
        for(int i = 0; i < trains.Count;i++)
        {
            float departureTime = trains[i].remainingTimeTillDeparture;
            if(departureTime > 120f && departureTime <= 240f)
            {
                return trains[i];
            }
        }

        return null;
    }
}
