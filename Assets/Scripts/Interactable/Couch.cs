using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Couch : MonoBehaviour
{

    public Transform seat1;

    public Transform seat2;

    public AutonomousPedestrian seat1Occupier;

    public AutonomousPedestrian seat2Occupier;

    public bool seat1Occupied;

    public bool seat2Occupied;

    private readonly object seatLock = new object();
    
    public int getNumberOfOccupiedSeats()
    {
        int amountOfOccupiedSeats = 0;

        if(seat1Occupied)
        {
            amountOfOccupiedSeats += 1;
        }

        if(seat2Occupied)
        {
            amountOfOccupiedSeats += 1;
        }

        return amountOfOccupiedSeats;
    }

    public Transform bestSeat(AutonomousPedestrian pedestrian)
    {
        lock (seatLock)
        {
            if(!seat1Occupied && !seat2Occupied)
            {
                int random = Random.Range(0, 2);
                if(random == 0)
                {
                    seat1Occupied = true;
                    seat1Occupier = pedestrian;
                    return seat1;
                }
                else
                {
                    seat2Occupied = true;
                    seat2Occupier = pedestrian;
                    return seat2;
                }
            }
            else if(!seat1Occupied)
            {
                seat1Occupied = true;
                seat1Occupier = pedestrian;
                return seat1;
            }
            else if(!seat2Occupied)
            {
                seat2Occupied = true;
                seat2Occupier = pedestrian;
                return seat2;
            }
            else
            {
                return null;
            }
        }

    }

    public void leaveSeat(Transform seat)
    {
        if(seat == seat1)
        {
            seat1Occupied = false;  
            seat1Occupier = null;
        }
        if(seat == seat2)
        {
            seat2Occupied = false;
            seat2Occupier = null;
        }
    }

    public void markAsOccupied(Transform seat, AutonomousPedestrian pedestrian)
    {
        if(seat == seat1)
        {
            seat1Occupied = true;  
            seat1Occupier = pedestrian;

        }
        if(seat == seat2)
        {
            seat2Occupied = true;
            seat2Occupier = pedestrian;
        }
    }

    public AutonomousPedestrian getOccupier(Transform seat)
    {
        if(seat == seat1)
        {
            return seat1Occupier;

        }
        else if(seat == seat2)
        {
            return seat2Occupier;
        }
        else
        {
            return null;
        }
    }

}
