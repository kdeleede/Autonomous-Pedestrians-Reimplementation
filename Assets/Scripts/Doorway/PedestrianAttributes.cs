using UnityEngine;
using UnityEngine.AI;

public class PedestrianAttributes: MonoBehaviour
{
    public bool rush;
    public bool effort;
    public bool kindness;
    public bool care;

    public float HOLProbability;
    public float HOFProbability;
    public float NHProbability;

    /*
    public void Start()
    {
        rush = false;
        effort = false;
        kindness = false;
        care = false;

        HOLProbability = .33f;
        HOFProbability = .33f;
        NHProbability = .33f;
    }*/
    public void InitializeAttributes(NavMeshAgent agent)
    {
        if (agent.speed >= 2.15f)
        {
            rush = true;
        }
        else
        {
            rush = false;
        }

        effort = UnityEngine.Random.value > 0.5f;
        kindness = UnityEngine.Random.value > 0.5f;
        care = UnityEngine.Random.value > 0.5f;

        SetProbabilities();
    }

    private void SetProbabilities()
    {
        if (rush)
        {
            if (effort)
            {
                if (care)
                {
                    if (kindness)
                    {
                        HOLProbability = 0.7f;
                        HOFProbability = 0.1f;
                        NHProbability = 0.2f;
                    }
                    else
                    {
                        HOLProbability = 0.6f;
                        HOFProbability = 0.1f;
                        NHProbability = 0.3f;
                    }
                }
                else
                {
                    if (kindness)
                    {
                        HOLProbability = 0.7f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.3f;
                    }
                    else
                    {
                        HOLProbability = 0.5f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.5f;
                    }
                }
            }
            else
            {
                if (care)
                {
                    if (kindness)
                    {
                        HOLProbability = 0.1f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.9f;
                    }
                    else
                    {
                        HOLProbability = 0.1f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.9f;
                    }
                }
                else
                {
                    if (kindness)
                    {
                        HOLProbability = 0.1f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.9f;
                    }
                    else
                    {
                        HOLProbability = 0.0f;
                        HOFProbability = 0.0f;
                        NHProbability = 1.0f;
                    }
                }
            }
        }
        else
        {
            if (effort)
            {
                if (care)
                {
                    if (kindness)
                    {
                        HOLProbability = 0.2f;
                        HOFProbability = 0.8f;
                        NHProbability = 0.0f;
                    }
                    else
                    {
                        HOLProbability = 0.7f;
                        HOFProbability = 0.2f;
                        NHProbability = 0.1f;
                    }
                }
                else
                {
                    if (kindness)
                    {
                        HOLProbability = 0.9f;
                        HOFProbability = 0.1f;
                        NHProbability = 0.0f;
                    }
                    else
                    {
                        HOLProbability = 0.6f;
                        HOFProbability = 0.1f;
                        NHProbability = 0.3f;
                    }
                }
            }
            else
            {
                if (care)
                {
                    if (kindness)
                    {
                        HOLProbability = 0.2f;
                        HOFProbability = 0.1f;
                        NHProbability = 0.7f;
                    }
                    else
                    {
                        HOLProbability = 0.5f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.5f;
                    }
                }
                else
                {
                    if (kindness)
                    {
                        HOLProbability = 0.3f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.7f;
                    }
                    else
                    {
                        HOLProbability = 0.1f;
                        HOFProbability = 0.0f;
                        NHProbability = 0.9f;
                    }
                }
            }
        }
    }
}
