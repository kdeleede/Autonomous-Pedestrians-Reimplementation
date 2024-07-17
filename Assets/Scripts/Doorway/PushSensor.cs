using UnityEngine;

public class PushSensor : MonoBehaviour
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

                    if(commuter.enabled)
                    {
                        commuter.inDoorwayRegion = true;
                        commuter.onPushSide = true;
                        commuter.isHolder = false;
                        commuter.isFollower = false;
                        commuter.waitingTime = 0;
                        commuter.door = door;
                        door.pedestriansFacingDoor.Add(pedestrian);
                        Debug.Log("COMMMUTE enbled PushInDoor");
                        commuter.selectInitialLeader(true);

                        commuter.OldDestination = commuter.agent.destination;
                        commuter.oldFulfillingDesire = commuter.isFullfillingDesire;
                    }
                    else if (exiter.enabled)
                    {
                        exiter.inDoorwayRegion = true;
                        exiter.onPushSide = true;
                        exiter.isHolder = false;
                        exiter.isFollower = false;
                        exiter.waitingTime = 0;
                        exiter.door = door;
                        door.pedestriansFacingDoor.Add(pedestrian);
                        Debug.Log("EXITER enabled PushInDoor");
                        exiter.selectInitialLeader(true);

                        exiter.OldDestination = exiter.agent.destination;
                        exiter.oldFulfillingDesire = exiter.isFullfillingDesire;
                    }
                }
            }
        }
    }

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
                        commuter.inDoorwayRegion = false;
                        commuter.onPushSide = false;
                        commuter.door = null;
                        door.pedestriansFacingDoor.Remove(pedestrian);
                        Debug.Log("PushOutDoor");

                        commuter.agent.SetDestination(commuter.OldDestination);
                        commuter.isFullfillingDesire = commuter.oldFulfillingDesire;
                        commuter.destinationSet = false;
                    }
                    else if (exiter.enabled == true)
                    {
                        if (exiter.leader != null && exiter.leader.currentDoorState == AutonomousPedestrian.DoorAction.LetFollowerPassFirst)
                        {
                            exiter.leader.currentDoorState = AutonomousPedestrian.DoorAction.HolderAwaitDecision;
                        }
                        exiter.inDoorwayRegion = false;
                        exiter.onPushSide = false;
                        exiter.door = null;
                        door.pedestriansFacingDoor.Remove(pedestrian);
                        Debug.Log("PushOutDoor");

                        exiter.agent.SetDestination(exiter.OldDestination);
                        exiter.isFullfillingDesire = exiter.oldFulfillingDesire;
                        exiter.destinationSet = false;  // Set destination to false
                    }
                }
            }
        }
    }
}
            
