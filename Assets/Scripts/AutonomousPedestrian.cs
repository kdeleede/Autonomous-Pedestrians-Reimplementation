using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Collections;

public class AutonomousPedestrian : MonoBehaviour
{
    
    protected NavMeshAgent agent;
    protected List<Vector3> waypoints;

    protected Animator animator;


    public float sensingDistance = 10f;

    public List<Transform> pointsOfInterest;

    [Header ("Layers")]

    public LayerMask ticketBoothLayer;


    [Header("Select Point")]
    public float destinationRadius = 5f; // Radius to find destination around points of interest
    public float maxSlope = 30f; // Maximum slope angle to consider for valid positions
    public int maxAttempts = 10;

    [Header("Desires")]

    protected bool isFullfillingDesire = false;

    protected float thirstRate = 1f;
    protected float maxThirstValue = 500f;

    protected float attractionRate = 1f;
    protected float maxAttractionValue = 500f;

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

    private readonly object behaviourLock = new object();


    [Header("Knowledge")]

    public List<Transform> ticketLocations;

    public List<Transform> concordLocations;

    public List<Transform> vendingMachineLocations;




    
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

    }
    
    

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.isStopped = false;

        thirstRate = UnityEngine.Random.Range(.7f, 1.3f);
        tiredRate = UnityEngine.Random.Range(.7f, 1.3f);
        attractionRate = UnityEngine.Random.Range(.7f, 1.3f);

        timeBeforeBeingHurried = UnityEngine.Random.Range(120f, 30f);
    }

    protected virtual void Update()
    {
        

        attraction += Time.deltaTime * attractionRate / maxTiredValue;
        //thirst += Time.deltaTime * thirstRate / maxThirstValue;
        thirst = 1.1f;
        tiredness += Time.deltaTime * tiredRate / maxTiredValue;
        
        
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
    
    /*
    protected virtual IEnumerator BehaviourControl()
    {
        while (true)
        {
            switch (currentAction)
            {
                case PedestrianAction.Idle:
                    break;
                case PedestrianAction.GoToTicketBooth:
                    Debug.Log("GoToTicketBooth");
                    isFullfillingDesire = true;
                    Transform selectedLocation;
                    if(ticketLocations.Count > 0)
                    {
                        selectedLocation = ticketLocations[UnityEngine.Random.Range(0, ticketLocations.Count)];
                        SetDestinationAroundPointOfInterest(selectedLocation.position);
                    }
                    else // NEED TO DISCOVER LOCATION
                    {

                    }
                    
                    
                    yield return new WaitForSeconds(1f);
                    currentAction = PedestrianAction.TravelingToTicketBooth;
                    break;
                case PedestrianAction.TravelingToTicketBooth:
                    Debug.Log("Traveling");
                    if (agent.remainingDistance < .5f)
                    {
                        currentAction = PedestrianAction.GetInTicketLine;
                    }
                    break;
                case PedestrianAction.GetInTicketLine:
                    Debug.Log("Get In Line");

                    GameObject currentBooth = null;
                    float minBoothCount = Mathf.Infinity;
                    List<GameObject> ticketBooths = senseObjects(ticketBoothLayer, 15f);
                    if (ticketBooths != null && ticketBooths.Count > 0)
                    {
                        for (int i = 0; i < ticketBooths.Count; i++)
                        {
                            TicketBooth booth = ticketBooths[i].GetComponent<TicketBooth>();
                            if (booth != null && booth.checkQueueLength() < minBoothCount)
                            {
                                currentBooth = ticketBooths[i];
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

                    currentAction = PedestrianAction.WaitInTicketLine;
                    break;
                case PedestrianAction.WaitInTicketLine:
                    Debug.Log("AcquiredTicket");
                    if (hasTicket == false)
                    {
                        if (selectedBooth != null)
                        {
                            int checkQueuePosition = selectedBooth.checkQueuePosition(this);
                            
                            if(currentQueuePosition != checkQueuePosition)
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
                            }
                            
                        }
                        else
                        {
                            Debug.LogError("Selected booth is null.");
                        }
                    }
                    else
                    {
                        agent.SetDestination(concordLocation.position);
                        yield return new WaitForSeconds(.1f);
                        currentAction = PedestrianAction.TravelingToConcord;
                    }
                    break;
                case PedestrianAction.TravelingToConcord:
                    if (agent.remainingDistance < .5f)
                    {
                        currentAction = PedestrianAction.Idle;
                    }
                    break;
            }
            yield return null; // Yielding null returns control to Unity and continues in the next frame
        }
    }
    */

    protected virtual IEnumerator BehaviourControl()
    {
        while (true)
        {
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
                    yield return GetInTicketLine();
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
                    yield return GetInTicketLine();
                    currentAction = PedestrianAction.WaitInVendingMachineLine;
                    break;

                case PedestrianAction.WaitInVendingMachineLine:
                    yield return WaitInVendingMachineLine();
                    break;
            }
        }
    }

    private IEnumerator GoToTicketBooth()
    {
        isFullfillingDesire = true;

        if (ticketLocations.Count > 0)
        {
            setDestinationFromLocations(ticketLocations, destinationRadius);
        }
        else
        {
            // Handle discovering location
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator GoToVendingMachine()
    {
        isFullfillingDesire = true;

        if (ticketLocations.Count > 0)
        {
            setDestinationFromLocations(vendingMachineLocations, destinationRadius);
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
        yield return null;
    }

    private IEnumerator GetInTicketLine()
    {
        GameObject currentBooth = null;
        float minBoothCount = Mathf.Infinity;
        List<GameObject> ticketBooths = senseObjects(ticketBoothLayer, 15f);

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
                }
            }
            else
            {
                Debug.LogError("Selected booth is null.");
            }
        }
        else
        {
            if(concordLocations.Count > 0)
            {
                setDestinationFromLocations(concordLocations, 15f);
            }
            else
            {
                // Discover Concord Location
            }
            yield return new WaitForSeconds(1f);
            currentAction = PedestrianAction.TravelingToConcord;
        }
    }

    private IEnumerator WaitInVendingMachineLine()
    {
        Debug.Log("WaitForVend");
        if (selectedBooth != null)
        {
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
                thirst = 0;
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

    protected void setDestinationFromLocations(List<Transform> locations, float destinationRadius)
    {
        Transform selectedLocation = locations[UnityEngine.Random.Range(0, locations.Count)];
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
