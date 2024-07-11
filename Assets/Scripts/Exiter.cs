using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exiter : AutonomousPedestrian
{
    protected override void Start()
    {
        base.Start();
        StartCoroutine(BehaviourControl());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(isFullfillingDesire == false)
        {
            currentAction = PedestrianAction.GoToExit;
        }
        else
        {

        }


        


    }
}
