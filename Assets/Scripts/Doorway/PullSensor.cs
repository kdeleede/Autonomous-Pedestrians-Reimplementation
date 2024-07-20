using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PullSensor : MonoBehaviour
{
    public Door door; // Reference to the parent door script
    public LayerMask pedestrianLayer; // Layer mask for detecting pedestrians

    void Start()
    {
        if (door == null)
        {
            door = GetComponentInParent<Door>();
        }
    }

    private IEnumerator setInDoorway(AutonomousPedestrian ped, bool setBool)
    {
        if(!setBool)
        {
            yield return new WaitForSeconds(.01f);
            ped.inDoorwayRegion = false;
            yield return new WaitForSeconds(.01f);
            ped.door = null;
            ped.agent.SetDestination(ped.OldDestination);
        }
        else
        {
            yield return new WaitForSeconds(.01f);
            ped.inDoorwayRegion = true;
        }
        //ped.currentAction = AutonomousPedestrian.PedestrianAction.GoToTicketBooth;
    }
    

    void OnTriggerEnter(Collider other)
    {
        if ((pedestrianLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            Commuter commuter = other.GetComponent<Commuter>();
            Exiter exiter = other.GetComponent<Exiter>();

            AutonomousPedestrian pedestrian;
            if(commuter.enabled == true)
            {
                pedestrian = commuter as AutonomousPedestrian;
            }
            else
            {
                pedestrian = exiter as AutonomousPedestrian;
            }
            
            if (pedestrian != null)
            {
                if(pedestrian.inDoorwayRegion != true)
                {

                    if(commuter.enabled == true)
                    {
                        commuter.OldDestination = commuter.agent.destination;
                        float distancePush = Vector3.Distance(door.pushExit.position, commuter.OldDestination);
                        float distancePull = Vector3.Distance(door.pullExit.position, commuter.OldDestination);
                        if(distancePush > distancePull)
                        {
                            commuter.onPullSide = true;
                            door.pedestriansFacingAwayFromDoor.Add(pedestrian);

                        }
                        else
                        {
                            commuter.onPushSide = true;
                            door.pedestriansFacingDoor.Add(pedestrian);
                        }
                        //commuter.onPullSide = true;
                        commuter.isHolder = false;
                        commuter.isFollower = false;
                        commuter.waitingTime = 0;
                        commuter.door = door;
                        Debug.Log("COMMMUTE enbled PushInDoor");
                        commuter.selectInitialLeader(true);

                        StartCoroutine(setInDoorway(commuter, true));
                        //commuter.inDoorwayRegion = true;
                        
                        //commuter.oldFulfillingDesire = commuter.isFullfillingDesire;
                    }
                    else if (exiter.enabled)
                    {
                        exiter.OldDestination = exiter.agent.destination;
                        float distancePush = Vector3.Distance(door.pushExit.position, exiter.OldDestination);
                        float distancePull = Vector3.Distance(door.pullExit.position, exiter.OldDestination);
                        if(distancePush > distancePull)
                        {
                            exiter.onPullSide = true;
                            door.pedestriansFacingAwayFromDoor.Add(pedestrian);
                        }
                        else
                        {
                            exiter.onPushSide = true;
                            door.pedestriansFacingDoor.Add(pedestrian);
                        }
                        //exiter.onPullSide = true;
                        exiter.isHolder = false;
                        exiter.isFollower = false;
                        exiter.waitingTime = 0;
                        exiter.door = door;
                        Debug.Log("EXITER enabled PushInDoor");
                        exiter.selectInitialLeader(true);

                        StartCoroutine(setInDoorway(exiter, true));
                        //exiter.inDoorwayRegion = true;

                        //exiter.oldFulfillingDesire = exiter.isFullfillingDesire;
                    }
                }
            }
        }
    }

    /*
    void OnTriggerExit(Collider other)
    {
        if ((pedestrianLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            Commuter commuter = other.GetComponent<Commuter>();
            Exiter exiter = other.GetComponent<Exiter>();

            AutonomousPedestrian pedestrian;
            if(commuter.enabled == true)
            {
                pedestrian = commuter as AutonomousPedestrian;
            }
            else
            {
                pedestrian = exiter as AutonomousPedestrian;
            }

            if (pedestrian != null)
            {
                if(pedestrian.inDoorwayRegion != false)
                {
                    if(commuter.enabled == true)
                    {
                        if(commuter.leader != null && commuter.leader.currentDoorState == AutonomousPedestrian.DoorAction.LetFollowerPassFirst)
                        {
                            commuter.leader.currentDoorState = AutonomousPedestrian.DoorAction.HolderAwaitDecision;
                        }
                        commuter.onPullSide = false;
                        //commuter.door = null;
                        door.unregister(pedestrian);
                        Debug.Log("PullOutDoor");

                        //commuter.agent.SetDestination(commuter.OldDestination);
                        commuter.isFullfillingDesire = false;
                        commuter.destinationSet = false;
                        //commuter.inDoorwayRegion = false;
                        StartCoroutine(setInDoorway(commuter, false));

                    }
                    else if (exiter.enabled == true)
                    {
                        if (exiter.leader != null && exiter.leader.currentDoorState == AutonomousPedestrian.DoorAction.LetFollowerPassFirst)
                        {
                            exiter.leader.currentDoorState = AutonomousPedestrian.DoorAction.HolderAwaitDecision;
                        }
                        exiter.onPullSide = false;
                        //exiter.door = null;
                        door.unregister(pedestrian);
                        Debug.Log("PushOutDoor");

                        //exiter.agent.SetDestination(exiter.OldDestination);
                        exiter.isFullfillingDesire = false;
                        exiter.destinationSet = false;
                        StartCoroutine(setInDoorway(exiter, false));
                        //exiter.inDoorwayRegion = false;
                        //setInDoorway(exiter);

                    }
                }
            }
        }
    }
    */
}
