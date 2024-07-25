using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianSeparation : MonoBehaviour
{
    public float sensingDistance = 5f;
    public float separationDistance = 2f;
    public float separationStrength = 5f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        MaintainSeparation();
    }

    void MaintainSeparation()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sensingDistance);
        List<NavMeshAgent> nearbyAgents = new List<NavMeshAgent>();

        foreach (var hitCollider in hitColliders)
        {
            NavMeshAgent nearbyAgent = hitCollider.GetComponent<NavMeshAgent>();
            if (nearbyAgent != null && nearbyAgent != agent)
            {
                Vector3 toAgent = nearbyAgent.transform.position - transform.position;
                if (Vector3.Dot(toAgent.normalized, agent.velocity.normalized) > 0.7f) // adjust this threshold as needed
                {
                    nearbyAgents.Add(nearbyAgent);
                }
            }
        }

        Vector3 separationForce = Vector3.zero;
        foreach (var nearbyAgent in nearbyAgents)
        {
            Vector3 toAgent = transform.position - nearbyAgent.transform.position;
            if (toAgent.magnitude < separationDistance)
            {
                separationForce += toAgent.normalized / toAgent.magnitude;
            }
        }

        if (separationForce != Vector3.zero)
        {
            Vector3 newDestination = agent.destination + separationForce * separationStrength;
            agent.SetDestination(newDestination);
        }
    }
}
