using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navmesh : MonoBehaviour
{
    public Navmesh Surface;
    public float AvoidancePredictionTime = 2;
    public int PathfindingIterationsPerFrame = 100;

    // Start is called before the first frame update
    private void Start()
    {
        Surface.AvoidancePredictionTime = AvoidancePredictionTime;
        Surface.PathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
    }
}
