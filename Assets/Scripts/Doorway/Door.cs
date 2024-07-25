using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Door : MonoBehaviour
{
    public HingeJoint hinge;
    public Rigidbody doorRigidbody;

    public LayerMask pedestrianLayer;
    public List<AutonomousPedestrian> pedestriansFacingDoor = new List<AutonomousPedestrian>();
    public List<AutonomousPedestrian> pedestriansFacingAwayFromDoor = new List<AutonomousPedestrian>();

    public Transform pushExit;

    public Transform pullExit;

    public Transform pushTarget;

    public Transform pullTarget;

    public Transform pushSpotFullyOpen;

    public Transform pullSpotFullyOpen;

    public Transform pushSpotHalfOpen;

    public Transform pullSpotHalfHalfOpen;

    public int doorID;

    

    void Start()
    {
        if (hinge == null)
        {
            hinge = GetComponent<HingeJoint>();
        }

        if (doorRigidbody == null)
        {
            doorRigidbody = GetComponent<Rigidbody>();
        }
        doorID = UnityEngine.Random.Range(0, 100);
    }

    void Update()
    {
        
        // Add any additional logic you need in the Update method
        if(pedestriansFacingDoor.Count > 0)
        {
            foreach(AutonomousPedestrian p in pedestriansFacingDoor)
            {
                p.door = this;
                p.updateLeader();
            }
        }

        if(pedestriansFacingAwayFromDoor.Count > 0)
        {
            foreach (AutonomousPedestrian p in pedestriansFacingAwayFromDoor)
            {
                p.door = this;
                p.updateLeader();
            }
        }
    }

    public int totalPedestrians()
    {
        return pedestriansFacingDoor.Count + pedestriansFacingAwayFromDoor.Count;
    }

    public void unregister(AutonomousPedestrian ped)
    {
        if(pedestriansFacingDoor.Contains(ped))
        {
            pedestriansFacingDoor.Remove(ped);
        }

        if(pedestriansFacingAwayFromDoor.Contains(ped))
        {
            pedestriansFacingAwayFromDoor.Remove(ped);
        }

        return;
    }

    public List<AutonomousPedestrian> GetPedestriansFacingDoor()
    {
        return pedestriansFacingDoor;
    }

    public List<AutonomousPedestrian> GetPedestriansFacingAwayFromDoor()
    {
        return pedestriansFacingAwayFromDoor;
    }

    void setHingeSpring(float targetAngle)
    {
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.targetPosition = targetAngle;
        hinge.spring = hingeSpring;
    }

   public IEnumerator exitDoor(AutonomousPedestrian ped)
    {
        if (ped != null)
        {
            if (ped.inDoorwayRegion != false)
            {
                if (ped.leader != null && ped.leader.currentDoorState == AutonomousPedestrian.DoorAction.LetFollowerPassFirst)
                {
                    ped.leader.currentDoorState = AutonomousPedestrian.DoorAction.HolderAwaitDecision;
                }
                ped.onPullSide = false;
                unregister(ped);
                //Debug.Log("PullOutDoor");

                //ped.isFullfillingDesire = false;
                ped.destinationSet = false;
                ped.leader = null;
                ped.currentDoorState = AutonomousPedestrian.DoorAction.Wait;
                
                yield return new WaitForSeconds(0.01f);
                ped.inDoorwayRegion = false;
                yield return new WaitForSeconds(0.01f);
                ped.door = null;
                ped.agent.SetDestination(ped.OldDestination);
            }
        }
        yield return null;
    }
    

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Draw lines for pedestrians facing the door
        foreach (AutonomousPedestrian pedestrian in pedestriansFacingDoor)
        {   
            if (pedestrian.leader != null)
            {
                Gizmos.DrawLine(pedestrian.transform.position, pedestrian.leader.transform.position);
            }
        }

        // Draw lines for pedestrians facing away from the door
        foreach (AutonomousPedestrian pedestrian in pedestriansFacingAwayFromDoor)
        {
            if (pedestrian.leader != null)
            {
                Gizmos.DrawLine(pedestrian.transform.position, pedestrian.leader.transform.position);
            }
        }
    }

}
