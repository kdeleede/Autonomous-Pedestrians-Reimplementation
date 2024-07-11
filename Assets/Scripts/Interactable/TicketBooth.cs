using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class TicketBooth : MonoBehaviour
{
    public int lineLength = 0;

    private float timeToGetTicket = 1f;

    private List<AutonomousPedestrian> queue = new List<AutonomousPedestrian>();
    void Start()
    {
        
    }

    public int checkQueueLength()
    {
        return lineLength;
    }

    public int checkQueuePosition(AutonomousPedestrian pedestrian)
    {
        for(int i = 0; i < queue.Count; i++)
        {
            if(queue[i] == pedestrian)
            {
                return i;
            }
        }
        
        return -1;
    }

    public void getInQueue(AutonomousPedestrian pedestrian)
    {
        
        if (pedestrian != null)
        {
            lineLength += 1;
            queue.Add(pedestrian);
        }
        else
        {
            Debug.LogError("Pedestrian is null.");
        }
    }

    public Vector3 getQueueTransformPosition(AutonomousPedestrian pedestrian)
    {
        Vector3 offset = new Vector3(0, 0, -1.5f);
        Vector3 StartingPosition = transform.position + new Vector3(0, 0, -1.5f);

        return StartingPosition + offset * checkQueuePosition(pedestrian);
    }

    public IEnumerator getTicket(AutonomousPedestrian autonomousPedestrian)
    {
        if(autonomousPedestrian != null)
        {
            yield return new WaitForSeconds(timeToGetTicket);
            autonomousPedestrian.hasTicket = true;
            lineLength -= 1;
            queue.Remove(autonomousPedestrian);
        }
        else
        {
            Debug.Log("Getting Ticket Error");
        }
    } 

    public void exitQueue(AutonomousPedestrian autonomousPedestrian)
    {
        lineLength -= 1;
        queue.Remove(autonomousPedestrian);
    }

    void Update()
    {
        
    }
}
