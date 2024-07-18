using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianManager : MonoBehaviour
{
    public static PedestrianManager instance;

    [Header("Pedestrian")]
    public List<GameObject> commuterVariants;

    [Header("Spawn Points")]
    public List<Transform> enterSpawnPoint;

    [Header("Pedestrian Knowledge")]
    public List<Transform> ticketLocations;
    public List<Transform> concordLocations;
    public List<Transform> vendingMachineLocations;
    public List<Transform> danceSpotLocations;
    public List<Transform> couchLocations;

    private int setPriority;

    private int counter;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        counter = 0;
        StartCoroutine(SpawnCommuter());
    }

    IEnumerator SpawnCommuter()
    {
        while (true)
        {   
            yield return new WaitForSeconds(.1f);
            SpawnNewCommuter();
            counter += 1;
            if(counter == 5)
            {
                SpawnExiter();
                counter = 0;
            }
            yield return new WaitForSeconds(1.3f);
        }
    }
    void SpawnExiter()
    {
        Train assignedTrain = TrainManager.instance.assignTrain();
        int commuterIndex = Random.Range(0, commuterVariants.Count);
        Vector3 spawnPoint = assignedTrain.position;
        GameObject newExiter = Instantiate(commuterVariants[commuterIndex], spawnPoint, Quaternion.identity);
        NavMeshAgent agent = newExiter.GetComponent<NavMeshAgent>();
        PedestrianAttributes attributes = newExiter.GetComponent<PedestrianAttributes>();
        
        if (agent != null)
        {
            agent.speed = Random.Range(2.05f, 2.25f);
            agent.avoidancePriority = Random.Range(25,75);
            setPriority = agent.avoidancePriority;
        }
        else
        {
            Debug.LogError("NavMeshAgent component is missing on the new exiter.");
        }

        Exiter exiterComponent = newExiter.GetComponent<Exiter>();
        if (exiterComponent != null && agent != null)
        {
            exiterComponent.exitLocations = new List<Transform>(enterSpawnPoint);

            attributes.InitializeAttributes(agent);

            exiterComponent.oldPriority = setPriority;

            exiterComponent.enabled = true;

            //Debug.Log("Commuter component successfully assigned and enabled for " + newExiter.name);
        }
        else
        {
            Debug.LogError("Pedestrian component is missing on the new commuter.");
        }
        
    }

    void SpawnNewCommuter()
    {
        if (enterSpawnPoint.Count > 0)
        {
            int spawnIndex = Random.Range(0, enterSpawnPoint.Count);
            Transform spawnPoint = enterSpawnPoint[spawnIndex];

            int commuterIndex = Random.Range(0, commuterVariants.Count);
            GameObject newCommuter = Instantiate(commuterVariants[commuterIndex], spawnPoint.position, spawnPoint.rotation);

            NavMeshAgent agent = newCommuter.GetComponent<NavMeshAgent>();
            PedestrianAttributes attributes = newCommuter.GetComponent<PedestrianAttributes>();
            if (agent != null)
            {
                agent.speed = Random.Range(2.05f, 2.25f);
                agent.avoidancePriority = Random.Range(25,75);
                setPriority = agent.avoidancePriority;
            }
            else
            {
                Debug.LogError("NavMeshAgent component is missing on the new commuter.");
            }
            

            Commuter pedestrianComponent = newCommuter.GetComponent<Commuter>();
            if (pedestrianComponent != null && agent != null)
            {
                pedestrianComponent.ticketLocations = new List<Transform>(ticketLocations);
                pedestrianComponent.vendingMachineLocations = new List<Transform>(vendingMachineLocations);
                pedestrianComponent.danceSpotLocations = new List<Transform>(danceSpotLocations);
                pedestrianComponent.couchLocations = new List<Transform>(couchLocations);

                attributes.InitializeAttributes(agent);

                pedestrianComponent.oldPriority = setPriority;

                pedestrianComponent.enabled = true;

                //Debug.Log("Commuter component successfully assigned and enabled for " + newCommuter.name);
            }
            else
            {
                Debug.LogError("Pedestrian component is missing on the new commuter.");
            }


            
        }
        else
        {
            Debug.LogError("No enter spawn points available.");
        }
    }

    void Update()
    {

    }
}
