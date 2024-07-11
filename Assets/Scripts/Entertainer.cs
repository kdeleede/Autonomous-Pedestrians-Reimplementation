using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Entertainer : AutonomousPedestrian
{

    protected override void Start()
    {
        base.Start();
        StartCoroutine(BehaviourControl());
    }

    protected override void Update()
    {
        base.Update();

        if(isFullfillingDesire == false)
        {
            currentAction = PedestrianAction.GoToDanceSpot;
        }
        else
        {

        }


        


    }


}
