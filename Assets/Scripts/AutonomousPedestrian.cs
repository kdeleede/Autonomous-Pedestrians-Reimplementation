using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Collections;
using System.Data.Common;

public class AutonomousPedestrian : MonoBehaviour
{
    [Header ("Attributes")]
    public NavMeshAgent agent;
    protected List<Vector3> waypoints;

    protected Animator animator;


    public float sensingDistance = 10f;

    public float rotationSpeed = 3.0f;

    public PedestrianAttributes pedestrianAttributes;

    [Header ("Layers")]

    public LayerMask ticketBoothLayer;

    public LayerMask entertainersLayer;

    public LayerMask couchLayer;

    public LayerMask vendingMachineLayer;


    [Header("Select Point")]
    public float destinationRadius = 5f; // Radius to find destination around points of interest
    public float maxSlope = 30f; // Maximum slope angle to consider for valid positions
    public int maxAttempts = 10;

    [Header("Desires")]

    public bool isFullfillingDesire = false;

    protected float thirstRate = 1f;
    protected float maxThirstValue = 500f;

    protected float attractionRate = 1f;
    protected float maxAttractionValue = 250f;

    protected float tiredRate = 1f;
    protected float maxTiredValue = 500f;

    protected float timeBeforeBeingHurried = 45f;
    
    protected float thirst;

    protected float attraction;

    protected float tiredness;

    protected float hurriedness;

    [Header("Ticket and Train Status")]
    public bool hasTicket = false;

    public Train assignedTrain;

    public TicketBooth selectedBooth;

    public int currentQueuePosition;


    [Header("Current Action")]
    public PedestrianAction currentAction = PedestrianAction.Idle;

    public bool isWatchingDance = false;

    public bool isSitting = false;

    public GameObject selectedEntertainer;

    private readonly object behaviourLock = new object();

    public Couch selectedCouch;
    public Transform selectedSeat;


    [Header("Knowledge")]

    public List<Transform> ticketLocations;

    public List<Transform> concordLocations;

    public List<Transform> vendingMachineLocations;

    public List<Transform> danceSpotLocations;

    public List<Transform> couchLocations;

    public List<Transform> exitLocations;

    [Header("Priority Variables")]

    public int oldPriority;

    [Header("Doorway Etiquette")]

    public Door door;
    public AutonomousPedestrian leader;

    
    public bool isHolder = false; // This pedestrian is the door holder
    public bool isFollower = false; // This pedestrian is a follower

    public bool inDoorwayRegion;

    public bool onPushSide;

    public bool onPullSide;


    public float waitingTime = 0f;

    public int id;

    public Vector3 OldDestination;

    public bool oldFulfillingDesire;

    public DoorAction currentDoorState = DoorAction.Wait;

    public bool awaitDecision = false;

    public bool destinationSet = false;





    
    public enum PedestrianAction
    {
        Idle,
        GoToTicketBooth,

        TravelingToTicketBooth,

        GetInTicketLine,

        WaitInTicketLine,

        TravelingToConcord,

        GoToVendingMachine,

        TravelingToVendingMachine,

        GetInVendingMachineLine,

        WaitInVendingMachineLine,

        GotToTrain,

        TravelingToTrain,

        GoToDanceSpot,

        TravelingToDanceSpot,

        GoToWatchDance,

        TravelingToWatchDance,

        WatchingDance,

        GoToSeat,

        TravelingToSeat,

        TravelingToSeatLocation,

        FindBestSeat,

        Sitting,

        GoToExit,

        TravelingToExit



    }

    public enum DoorAction
    {
        Wait,

        LeaderApproachDoor,

        FollowerApproachDoor,

        FollowerWaitForLeaderCommand,

        ApproachDoor,
        HoldDoor,

        TravelingToHoldDoor,

        Exiting,

        HolderAwaitDecision,

        LetFollowerPassFirst,

        ExitWithoutFollower,

        MakeFollowerHolder,

        FollowerSwitchingToHolder,

        Idle,
    }
    
    

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        pedestrianAttributes = GetComponent<PedestrianAttributes>();

        agent.isStopped = false;

        thirstRate = UnityEngine.Random.Range(.7f, 1.3f);
        tiredRate = UnityEngine.Random.Range(.7f, 1.3f);
        attractionRate = UnityEngine.Random.Range(.7f, 1.3f);

        thirst = UnityEngine.Random.Range(0f,1f);
        tiredness = UnityEngine.Random.Range(0f,1f);
        attraction = UnityEngine.Random.Range(0f,1f);

        timeBeforeBeingHurried = UnityEngine.Random.Range(120f, 30f);

        id = UnityEngine.Random.Range(0,100);
    }

    protected virtual void Update()
    {
        if(inDoorwayRegion)
        {
            waitingTime += Time.deltaTime;
        }
        else
        {
            waitingTime = 0;
            currentDoorState = DoorAction.Wait;
        }
        

        if(!isWatchingDance)
        {
            attraction += Time.deltaTime * attractionRate / maxAttractionValue;
        }
        else
        {
            attraction -= Time.deltaTime * attractionRate / maxAttractionValue;
            if (selectedEntertainer != null)
            {
                Vector3 directionToEntertainer = (selectedEntertainer.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToEntertainer);

                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
        attraction = Mathf.Clamp(attraction, 0, 1);

        thirst += Time.deltaTime * thirstRate / maxThirstValue;
        thirst = Mathf.Min(thirst, 1);

        if(!isSitting)
        {
            tiredness += Time.deltaTime * tiredRate / maxTiredValue;
            agent.avoidancePriority = oldPriority;
            if(agent.avoidancePriority != oldPriority)
            {
                agent.avoidancePriority = oldPriority;
            }
        }
        else
        {
            tiredness -= Time.deltaTime * tiredRate / maxTiredValue;
            if (selectedSeat != null)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, selectedSeat.rotation, Time.deltaTime * rotationSpeed);
            }

            if(agent.avoidancePriority != 0)
            {
                agent.avoidancePriority = 0;
            }
        }
        tiredness = Mathf.Clamp(tiredness, 0, 1);
        
        if(isHolder && inDoorwayRegion)
        {
            agent.avoidancePriority = 0;
        }
        
        else if(inDoorwayRegion)
        {
            agent.avoidancePriority = 2;
        }
        else
        {
            agent.avoidancePriority = oldPriority;
        }
        if(assignedTrain != null)
        {
            if(assignedTrain.remainingTimeTillDeparture <= timeBeforeBeingHurried)
            {
                hurriedness = 1;
            }
            else
            {
                hurriedness = 0;
            }
        }
        else
        {
            hurriedness = 0;
        }

        UpdateAnimation();
    }

    protected virtual IEnumerator BehaviourControl()
    {
        while (true)
        {
            if(inDoorwayRegion)
            {
                yield return inDoorWayBehaviours();
            }
            else
            {
                    
                if(isWatchingDance == true)
                {
                    if(currentAction != PedestrianAction.GoToWatchDance)
                    {
                        isWatchingDance = false;
                    }
                    else
                    {
                        currentAction = PedestrianAction.WatchingDance;
                    }
                }

                if(isSitting == true)
                {
                    if(currentAction != PedestrianAction.GoToSeat)
                    {
                        isSitting = false;
                        if(selectedCouch != null && selectedSeat != null)
                        {
                            selectedCouch.leaveSeat(selectedSeat);
                        }
                        animator.SetBool("Sitting", false);
                    }
                    else
                    {
                        currentAction = PedestrianAction.Sitting;
                        if(selectedCouch != null && selectedSeat != null)
                        {
                            selectedCouch.markAsOccupied(selectedSeat, this);
                        }
                    }
                }

                switch (currentAction)
                {
                    case PedestrianAction.Idle:
                        yield return null;
                        break;

                    case PedestrianAction.GoToTicketBooth:
                        yield return GoToTicketBooth();
                        currentAction = PedestrianAction.TravelingToTicketBooth;
                        break;

                    case PedestrianAction.TravelingToTicketBooth:
                        yield return TravelingToTicketBooth();
                        break;

                    case PedestrianAction.GetInTicketLine:
                        yield return GetInTicketLine(ticketBoothLayer);
                        currentAction = PedestrianAction.WaitInTicketLine;
                        break;

                    case PedestrianAction.WaitInTicketLine:
                        yield return WaitInTicketLine();
                        break;

                    case PedestrianAction.TravelingToConcord:
                        yield return TravelingToConcord();
                        break;

                    case PedestrianAction.GoToVendingMachine:
                        yield return GoToVendingMachine();
                        currentAction = PedestrianAction.TravelingToVendingMachine;
                        break;

                    case PedestrianAction.TravelingToVendingMachine:
                        yield return TravelingToVendingMachine();
                        break;

                    case PedestrianAction.GetInVendingMachineLine:
                        yield return GetInTicketLine(vendingMachineLayer);
                        currentAction = PedestrianAction.WaitInVendingMachineLine;
                        break;

                    case PedestrianAction.WaitInVendingMachineLine:
                        yield return WaitInVendingMachineLine();
                        break;

                    case PedestrianAction.GotToTrain:
                        isFullfillingDesire = true;
                        yield return GotToTrain();
                        currentAction = PedestrianAction.TravelingToTrain;
                        break;

                    case PedestrianAction.TravelingToTrain:
                        yield return TravelingToTrain();
                        break;

                    case PedestrianAction.GoToDanceSpot:
                        isFullfillingDesire = true;
                        yield return GoToLocation(danceSpotLocations, true, 3f);
                        currentAction = PedestrianAction.TravelingToDanceSpot;
                        break;

                    case PedestrianAction.TravelingToDanceSpot:
                        yield return TravelingToDanceSpot();
                        break;

                    case PedestrianAction.GoToWatchDance:
                        isFullfillingDesire = true;
                        yield return GoToLocation(danceSpotLocations, false, 4f);
                        currentAction = PedestrianAction.TravelingToWatchDance;
                        break;
                    
                    case PedestrianAction.TravelingToWatchDance:
                        yield return TravelingToWatchDance();
                        break;

                    case PedestrianAction.WatchingDance:
                        yield return WatchingDance();
                        break;

                    case PedestrianAction.GoToSeat:
                        isFullfillingDesire = true;
                        yield return GoToLocation(couchLocations, false, destinationRadius);
                        currentAction = PedestrianAction.TravelingToSeatLocation;
                        break;

                    case PedestrianAction.TravelingToSeatLocation:
                        yield return TravelingToSeatLocation();
                        break;
                    
                    case PedestrianAction.FindBestSeat:
                        yield return FindBestSeat();
                        break;

                    case PedestrianAction.TravelingToSeat:
                        yield return TravelingToSeat();
                        break;

                    case PedestrianAction.Sitting:
                        yield return Sitting();
                        break;

                    case PedestrianAction.GoToExit:
                        yield return GoToLocation(exitLocations, false, destinationRadius);
                        currentAction = PedestrianAction.TravelingToExit;
                        break;

                    case PedestrianAction.TravelingToExit:
                        yield return TravelingToExit();
                        break;
                    
                }
            }
        }
    }

    private IEnumerator inDoorWayBehaviours()
    {
        isFullfillingDesire = true;

        switch (currentDoorState)
        {
            case DoorAction.Wait:
                yield return DoorActionWait();
                break;
            case DoorAction.LeaderApproachDoor:
                if(leader != null)
                {
                    currentDoorState = DoorAction.Wait;
                }
                yield return DoorActionApproachDoor();
                break;
            case DoorAction.FollowerApproachDoor:
                yield return DoorActionApproachDoor();
                break;
            case DoorAction.HoldDoor:
                yield return HoldDoorWait();
                break;
            case DoorAction.FollowerWaitForLeaderCommand:
                yield return FollowerWaitForLeaderCommand();
                break;
            case DoorAction.ExitWithoutFollower:
                yield return ExitWithoutFollower();
                break;

            case DoorAction.HolderAwaitDecision:
                yield return HolderMakeDecision();
                break;

            case DoorAction.LetFollowerPassFirst:
                if(door.totalPedestrians() == 1)
                {
                    currentDoorState = DoorAction.ExitWithoutFollower;
                }
                yield return HoldDoorWait();
                break;

            case DoorAction.MakeFollowerHolder:
                if(door.totalPedestrians() == 1)
                {
                    currentDoorState = DoorAction.ExitWithoutFollower;
                }
                yield return HoldDoorWait();
                break;

            case DoorAction.FollowerSwitchingToHolder:
                yield return FollowerSwitchingToHolder();
                break;

            case DoorAction.Idle:
                yield return null;
                break;
        }
    }

    private IEnumerator FollowerSwitchingToHolder()
    {
        if(agent.remainingDistance < .5f)
        {
            currentDoorState = DoorAction.HolderAwaitDecision;
            if(leader != null)
            {
                leader.currentDoorState = DoorAction.ExitWithoutFollower;
            }
        }
        yield return null;
    }

    private IEnumerator ExitWithoutFollower()
    {
        if(onPushSide)
        {
            agent.SetDestination(door.pushExit.position);
        }
        else
        {
            agent.SetDestination(door.pullExit.position);
        }
        yield return null;
    }

    
    private IEnumerator HoldDoorWait()
    {
        if (!destinationSet)
        {
            Vector3 destination;

            if (leader != null)
            {
                float distanceToPushSpot = Vector3.Distance(leader.transform.position, door.pushSpotFullyOpen.position);
                float distanceToPullSpot = Vector3.Distance(leader.transform.position, door.pullSpotFullyOpen.position);

                if (distanceToPushSpot < distanceToPullSpot)
                {
                    destination = door.pushSpotFullyOpen.position;
                }
                else
                {
                    destination = door.pullSpotFullyOpen.position;
                }

                
            }
            else
            {
                if (onPushSide)
                {
                    destination = door.pushSpotFullyOpen.position;
                }
                else
                {  
                    destination = door.pullSpotFullyOpen.position;
                }
            }

            agent.SetDestination(destination);
            destinationSet = true;
        }
        //currentDoorState = DoorAction.Idle;
        yield return null;
    }

    private IEnumerator DoorActionWait()
    {
        if(agent.destination != transform.position)
        {
            yield return new WaitForSeconds(.9f);
            if(agent.destination != transform.position)
            {
                agent.SetDestination(transform.position);
            }
        }
        if(leader == null)
        {
            currentDoorState = DoorAction.LeaderApproachDoor;
        }
        else
        {
            if(leader.isHolder == true)
            {
                isFollower = true;
                currentDoorState = DoorAction.FollowerApproachDoor;
            }
        }
        yield return null;   
    }

    private IEnumerator DoorActionApproachDoor()
    {
        
        if(onPushSide)
        {
            agent.SetDestination(door.pushExit.position);
        }
        else
        {
            agent.SetDestination(door.pullExit.position);
        }
        float distanceToDoor = Vector3.Distance(transform.position, door.transform.position);
        if(distanceToDoor < 2f)
        {
            if(leader == null)
            {
                currentDoorState = DoorAction.HolderAwaitDecision;
                //isHolder = true;
                //currentDoorState = DoorAction.HoldDoor;
                //HolderMakeDecision();
            }
            else if(isFollower)
            {
                currentDoorState = DoorAction.FollowerWaitForLeaderCommand;
                //agent.SetDestination(transform.position);
            }
            //
        }
        yield return null;
    }

    private IEnumerator HolderMakeDecision()
    {
        isHolder = true;
        //Depending on kindess
        float randomVal = UnityEngine.Random.value;
        Debug.Log("Decision Made");
        if(randomVal <= pedestrianAttributes.HOFProbability)
        {
            if(door.totalPedestrians() == 1)
            {
                currentDoorState = DoorAction.ExitWithoutFollower;
            }
            else
            {
                currentDoorState = DoorAction.LetFollowerPassFirst;
            }
        }
        else if(randomVal <= pedestrianAttributes.HOFProbability + pedestrianAttributes.HOLProbability)
        {
            if(door.totalPedestrians() == 1)
            {
                currentDoorState = DoorAction.ExitWithoutFollower;
            }
            else
            {
                currentDoorState = DoorAction.MakeFollowerHolder;
            }
        }
        else
        {
            currentDoorState = DoorAction.ExitWithoutFollower;
        }

        yield return null;
    }

    private IEnumerator FollowerWaitForLeaderCommand()
    {
        if(leader.currentDoorState == DoorAction.LetFollowerPassFirst)
        {
            if(onPushSide)
            {
                agent.SetDestination(door.pushExit.position);
            }
            else
            {
                agent.SetDestination(door.pullExit.position);
            }
        }
        else if(leader.currentDoorState == DoorAction.MakeFollowerHolder)
        {
            Vector3 destination;

            float distanceToPushSpot = Vector3.Distance(leader.transform.position, door.pushSpotFullyOpen.position);
            float distanceToPullSpot = Vector3.Distance(leader.transform.position, door.pullSpotFullyOpen.position);

            if (distanceToPushSpot < distanceToPullSpot)
            {
                destination = door.pushSpotHalfOpen.position;
            }
            else
            {
                destination = door.pullSpotHalfHalfOpen.position;
            }

            agent.SetDestination(destination);
            currentDoorState = DoorAction.FollowerSwitchingToHolder;
        }
        yield return null;
    }


    private IEnumerator TravelingToExit()
    {
        if (agent.remainingDistance < .5f)
        {
            Destroy(gameObject);
        }
        yield return null;
    }
    private IEnumerator FindBestSeat()
    {
        List<GameObject> sensedObjects = senseObjects(couchLayer, sensingDistance);

        Couch bestCouch = null;
        if(sensedObjects != null && sensedObjects.Count > 0)
        {
            int smallestCount = int.MaxValue;
            
            for(int i = 0; i < sensedObjects.Count; i++)
            {
                Couch couch = sensedObjects[i].GetComponent<Couch>();
                int availableSeats = couch.getNumberOfOccupiedSeats();
                if(availableSeats < smallestCount)
                {
                    smallestCount = availableSeats;
                    bestCouch = couch;
                }
            }
        }

        if(bestCouch != null)
        {
            if(bestCouch.getNumberOfOccupiedSeats() < 2)
            {
                selectedSeat = bestCouch.bestSeat(this);
                if(selectedSeat == null)
                {
                    currentAction = PedestrianAction.GoToSeat;
                }
                else
                {
                    selectedCouch = bestCouch;
                    agent.SetDestination(selectedSeat.position);
                    currentAction = PedestrianAction.TravelingToSeat;
                    yield return new  WaitForSeconds(0.5f);
                }
            }
            else
            {
                currentAction = PedestrianAction.GoToSeat;
            }
        }
        else
        {
            currentAction = PedestrianAction.GoToSeat;
        }

        yield return null;
    }
    private IEnumerator TravelingToSeatLocation()
    {
        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.FindBestSeat;
        }

        
        List<GameObject> sensedObjects = senseObjects(couchLayer, sensingDistance - 4f);

        if(sensedObjects != null && sensedObjects.Count > 1)
        {
            for(int i = 0; i < sensedObjects.Count; i++)
            {
                Couch couch = sensedObjects[i].GetComponent<Couch>();
                if(couch.getNumberOfOccupiedSeats() < 2)
                {
                    currentAction = PedestrianAction.FindBestSeat;
                }
            }
        }
        

        yield return null;
    }

    private IEnumerator TravelingToSeat()
    {
        if(selectedSeat != null && selectedCouch != null)
        {
            if(selectedCouch.getOccupier(selectedSeat) != this)
            {
                currentAction = PedestrianAction.GoToSeat;
            }
        }

        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.Sitting;
        }
        yield return null;
    }
    
    private IEnumerator Sitting()
    {
        isSitting = true;

        animator.SetBool("Sitting", true);
        
        yield return new WaitForSeconds(2.5f);
        isFullfillingDesire = false;
    }

    private IEnumerator WatchingDance()
    {
        if(agent.destination != transform.position)
        {
            agent.SetDestination(transform.position);
        }
        isWatchingDance = true;

        
        
        yield return new WaitForSeconds(5f);
        isFullfillingDesire = false;
    }
    private IEnumerator TravelingToWatchDance()
    {
        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.GoToWatchDance;
        }

        List<GameObject> sensedObjects = senseObjects(entertainersLayer, 4f);

        if(sensedObjects != null && sensedObjects.Count > 0)
        {
            currentAction = PedestrianAction.WatchingDance;
            selectedEntertainer = sensedObjects[0];
        }

        yield return null;
    }

    private IEnumerator GoToLocation(List<Transform> locations, bool chooseClosest, float destinationRadius)
    {
        isFullfillingDesire = true;
        if (locations.Count > 0)
        {
            setDestinationFromLocations(locations, destinationRadius, chooseClosest);
        }
        yield return new WaitForSeconds(.5f);
    }
    
    private IEnumerator TravelingToDanceSpot()
    {
        if (agent.remainingDistance < .5f)
        {
            animator.SetBool("Dancing", true);
            currentAction = PedestrianAction.Idle;
        }
        yield return null;
    }
    private IEnumerator TravelingToTrain()
    {
        if (agent.remainingDistance < .5f)
        {
            Destroy(gameObject);
        }
        yield return null;
    }

    private IEnumerator GotToTrain()
    {
        if(assignedTrain != null)
        {
            SetDestinationAroundPointOfInterest(assignedTrain.position, destinationRadius);
        }
        yield return new WaitForSeconds(.5f);
    }

    private IEnumerator GoToTicketBooth()
    {
        isFullfillingDesire = true;
        if(ticketLocations != null)
        {
            if (ticketLocations.Count > 0)
            {
                setDestinationFromLocations(ticketLocations, destinationRadius, false);
            }
            else
            {
                // Handle discovering location
            }

            yield return new WaitForSeconds(.5f);
        }

    }

    private IEnumerator GoToVendingMachine()
    {
        isFullfillingDesire = true;

        if (ticketLocations.Count > 0)
        {
            setDestinationFromLocations(vendingMachineLocations, destinationRadius, true);
        }
        else
        {
            // Handle discovering location
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator TravelingToTicketBooth()
    {
        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.GetInTicketLine;
        }
        yield return null;
    }

    private IEnumerator TravelingToVendingMachine()
    {
        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.GetInVendingMachineLine;
        }

        
        List<GameObject> lines = senseObjects(vendingMachineLayer, 5f);
        if(lines != null && lines.Count > 0)
        {
            currentAction = PedestrianAction.GetInVendingMachineLine;
            agent.SetDestination(transform.position);
        }
        
        yield return null;

        
        /*

        List<GameObject> lines = senseObjects(ticketBoothLayer, 10f);
        if(lines != null)
        {
            GetInTicketLine();
            currentAction = PedestrianAction.WaitInVendingMachineLine;
            agent.SetDestination(transform.position);
        }
        yield return null;
        */
    }

    private IEnumerator GetInTicketLine(LayerMask layerMask)
    {
        GameObject currentBooth = null;
        float minBoothCount = Mathf.Infinity;
        List<GameObject> ticketBooths = senseObjects(layerMask, 15f);

        if (ticketBooths != null && ticketBooths.Count > 0)
        {
            foreach (var boothObject in ticketBooths)
            {
                TicketBooth booth = boothObject.GetComponent<TicketBooth>();
                if (booth != null && booth.checkQueueLength() < minBoothCount)
                {
                    currentBooth = boothObject;
                    minBoothCount = booth.checkQueueLength();
                }
            }

            if (currentBooth != null)
            {
                selectedBooth = currentBooth.GetComponent<TicketBooth>();
                if (selectedBooth != null)
                {
                    selectedBooth.getInQueue(this);
                    agent.SetDestination(selectedBooth.getQueueTransformPosition(this));
                    currentQueuePosition = selectedBooth.checkQueuePosition(this);
                    yield return new WaitForSeconds(.5f);
                }
                else
                {
                    Debug.LogError("Selected booth is null.");
                }
            }
            else
            {
                Debug.LogError("No valid ticket booth found.");
            }
        }
        else
        {
            Debug.LogError("No ticket booths detected.");
        }
    }

    private IEnumerator WaitInTicketLine()
    {
        if (!hasTicket)
        {
            if (selectedBooth != null)
            {
                Vector3 directionToBooth = (selectedBooth.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToBooth);

                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

                int checkQueuePosition = selectedBooth.checkQueuePosition(this);

                if (currentQueuePosition != checkQueuePosition)
                {
                    agent.SetDestination(selectedBooth.getQueueTransformPosition(this));
                    currentQueuePosition = checkQueuePosition;
                    yield return new WaitForSeconds(1f);
                }
                else if (checkQueuePosition == 0 && agent.remainingDistance < 0.1f)
                {
                    yield return new WaitForSeconds(6f);
                    selectedBooth.exitQueue(this);
                    hasTicket = true;
                    assignedTrain = TrainManager.instance.assignTrain();
                    selectedBooth = null;
                    currentQueuePosition = 0; 
                    isFullfillingDesire = false;
                }
            }
            else
            {
                Debug.LogError("Selected booth is null.");
            }
        }
    }

    private IEnumerator WaitInVendingMachineLine()
    {
        //Debug.Log("WaitForVend");
        if (selectedBooth != null)
        {
            Vector3 directionToBooth = (selectedBooth.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToBooth);

            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            
            int checkQueuePosition = selectedBooth.checkQueuePosition(this);

            if (currentQueuePosition != checkQueuePosition)
            {
                agent.SetDestination(selectedBooth.getQueueTransformPosition(this));
                currentQueuePosition = checkQueuePosition;
                yield return new WaitForSeconds(1f);
            }
            else if (checkQueuePosition == 0 && agent.remainingDistance < 0.1f)
            {
                yield return new WaitForSeconds(6f);
                selectedBooth.exitQueue(this);
                thirst = thirst / 2;
                isFullfillingDesire = false;
                currentAction = PedestrianAction.Idle;
            }
        }
        else
        {
            Debug.LogError("Selected booth is null.");
        }
    }

    private IEnumerator TravelingToConcord()
    {
        if (agent.remainingDistance < .5f)
        {
            currentAction = PedestrianAction.Idle;
            isFullfillingDesire = false;
        }
        yield return null;
    }













    // HELPER FUNCTIONS

    public void selectInitialLeader(bool sameSide)
    {
        float sideDotProduct = Vector3.Dot(door.transform.forward, (transform.position - door.transform.position).normalized);
        List<AutonomousPedestrian> comparablePedestrians;
        if (sideDotProduct > 0)
        {
            if(sameSide)
            {
                comparablePedestrians = door.GetPedestriansFacingDoor();
            }
            else
            {
                comparablePedestrians = door.GetPedestriansFacingAwayFromDoor();
            }
        }
        else
        {
            if(sameSide)
            {
                comparablePedestrians = door.GetPedestriansFacingAwayFromDoor();
            }
            else
            {
                comparablePedestrians = door.GetPedestriansFacingDoor();
            }

        }

        AutonomousPedestrian initialLeader = null;

        float longestWaitingTime = 0;

        foreach (AutonomousPedestrian pedestrian in comparablePedestrians)
        {
            float waitingTime = pedestrian.waitingTime;
            
            if (waitingTime >= longestWaitingTime)
            {
                longestWaitingTime = waitingTime;
                initialLeader = pedestrian;
            }
        }

        leader = initialLeader;
        
        if(leader != null)
        {
            if(leader == this || leader.waitingTime < this.waitingTime)
            {
                leader = null;
            }
        }
    }
    public void updateLeader()
    {
        List<AutonomousPedestrian> pedestriansOnSideA = door.GetPedestriansFacingDoor();
        List<AutonomousPedestrian> pedestriansOnSideB = door.GetPedestriansFacingAwayFromDoor();
        
        // Determine which side this pedestrian is on
        float sideDotProduct = Vector3.Dot(door.transform.forward, (transform.position - door.transform.position).normalized);

        List<AutonomousPedestrian> sameSidePedestrians;
        List<AutonomousPedestrian> oppositeSidePedestrians;

        if (sideDotProduct > 0)
        {
            sameSidePedestrians = pedestriansOnSideA;
            oppositeSidePedestrians = pedestriansOnSideB; 
        }
        else
        {
            sameSidePedestrians = pedestriansOnSideB;
            oppositeSidePedestrians = pedestriansOnSideA;
        }

       
        if(leader != null && !sameSidePedestrians.Contains(leader) && !oppositeSidePedestrians.Contains(leader))
        {
            selectInitialLeader(true);//selectInitialLeader(true);
        }
        

        if(leader != null) // if leader exists
        {
            foreach(AutonomousPedestrian p in sameSidePedestrians)
            {
                if (p == this) continue;
            
                if(p.leader == leader)
                {
                    if(p.waitingTime >= waitingTime)
                    {
                        leader = p;
                    }
                }
            }
            foreach(AutonomousPedestrian p in oppositeSidePedestrians)
            {
                if(p == this) continue; 
                if(p.leader == leader)
                {
                    if(p.waitingTime >= waitingTime)
                    {
                        leader = p;
                    }
                }
            }

            /*
            foreach (AutonomousPedestrian pedestrian in sameSidePedestrians)
            {
                if (pedestrian == this) continue;

                if(pedestrian.leader == leader)
                {
                    if(pedestrian.waitingTime >= waitingTime) // compete
                    {
                        leader = pedestrian;
                        Debug.Log("B " + id.ToString() + " " + leader.id.ToString());
                        //if(leader.isHolder || leader.isFollower)
                        {
                            foreach (AutonomousPedestrian pedestrianOtherSide in oppositeSidePedestrians)
                            {
                                if(pedestrianOtherSide.leader == leader)
                                {
                                    if(pedestrianOtherSide.waitingTime >= waitingTime)
                                    {
                                        leader = pedestrian;
                                        Debug.Log("C " + id.ToString() + " " + leader.id.ToString());
                                    }
                                    else
                                    {
                                        //pedestrianOtherSide.leader = this;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //pedestrian.leader = this;
                    }
                }
                
            }
            */
        }
        else
        {
            selectInitialLeader(false);
            

            if(leader != null)
            {
                /*if(leader.waitingTime < waitingTime)
                {
                    leader = null;
                }
                */
                //else
                {   
                    Debug.Log("yeah");
                    foreach(AutonomousPedestrian p in sameSidePedestrians)
                    {
                        if(p == this) continue;

                        if(p.leader == leader)
                        {
                            if(p.waitingTime >= waitingTime)
                            {
                                leader = p;
                            }
                        }
                    }
                    foreach(AutonomousPedestrian p in oppositeSidePedestrians)
                    {
                        if(p.leader == leader)
                        {
                            if(p.waitingTime >= waitingTime)
                            {
                                leader = p;
                            }
                        }
                    }
                    /*
                    Debug.Log("D 2 ");
                    foreach (AutonomousPedestrian pedestrianOtherSide in oppositeSidePedestrians)
                    {
                        if(pedestrianOtherSide.leader == leader)
                        {
                            Debug.Log("D 3 ");
                            if(pedestrianOtherSide.waitingTime >= waitingTime)
                            {
                                leader = pedestrianOtherSide;
                                Debug.Log("E " + id.ToString() + " " + leader.id.ToString());
                            }
                            else
                            {
                                //pedestrianOtherSide.leader = this;
                            }
                        }
                    }
                    */
                }
            }
        }
    }

    protected void compete(AutonomousPedestrian pedestrianA, AutonomousPedestrian pedestrianB)
    {
        if(pedestrianA.leader == pedestrianB.leader)
        {
            if(pedestrianA.waitingTime >= pedestrianB.waitingTime)
            {
                pedestrianB.leader = pedestrianA;
            }
            else
            {
                pedestrianA.leader = pedestrianB;
            }
        }
    }

    protected List<GameObject> senseObjects(LayerMask sensingLayer, float sensingDistance)
    {
        List<GameObject> detectedObjects = new List<GameObject>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sensingDistance, sensingLayer);
        foreach (var hitCollider in hitColliders)
        {
            detectedObjects.Add(hitCollider.gameObject);
        }

        return detectedObjects;
    }


    protected void UpdateAnimation()
    {
        float speed = Mathf.Min(3f, agent.velocity.magnitude);
        animator.SetFloat("Speed", speed);
    }

    protected void setDestinationFromLocations(List<Transform> locations, float destinationRadius, bool chooseClosest)
    {
        Transform selectedLocation = null;

        if(chooseClosest)
        {
            // Initialize the minimum distance with a large value
            float minDistance = float.MaxValue;
            // Get the current position
            Vector3 currentPosition = transform.position;

            // Iterate through each location to find the closest one
            foreach (Transform location in locations)
            {
                // Calculate the distance to the current location
                float distance = Vector3.Distance(currentPosition, location.position);

                // If this distance is smaller than the current minimum distance, update the minimum distance and selected location
                if (distance < minDistance)
                {
                    minDistance = distance;
                    selectedLocation = location;
                }
            }
        }
        else
        {
            selectedLocation = locations[UnityEngine.Random.Range(0, locations.Count)];
        }
        SetDestinationAroundPointOfInterest(selectedLocation.position, destinationRadius);
    }

    protected void SetDestinationAroundPointOfInterest(Vector3 position, float destinationRadius)
    {
        int attempts = 0;
        bool destinationSet = false;

        while (attempts < maxAttempts && !destinationSet)
        {
            //Transform pointOfInterest = pointsOfInterest[UnityEngine.Random.Range(0, pointsOfInterest.Count)];
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * destinationRadius;
            randomDirection += position;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomDirection, out navHit, destinationRadius, NavMesh.AllAreas))
            {
                if (Vector3.Angle(Vector3.up, navHit.normal) <= maxSlope)
                {
                    agent.SetDestination(navHit.position);
                    destinationSet = true;
                }
            }

            attempts++;
        }

        if (!destinationSet)
        {
            Debug.LogWarning("Failed to set a valid destination around points of interest after maximum attempts.");
        }
    }




    // DEBUG
    protected void OnDrawGizmos()
    {
        // Draw waypoints as sphere gizmos
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 waypoint in waypoints)
            {
                Gizmos.DrawSphere(waypoint, 0.2f);
            }
        }
    }
}
