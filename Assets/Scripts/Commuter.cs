using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Commuter : AutonomousPedestrian
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
            if(hasTicket == false)
            {
                currentAction = PedestrianAction.GoToTicketBooth;
            }
            else // Pick most urgent desire
            {
                /*
                if(hurriedness == 1)
                {
                    // Go to train
                    Debug.Log("hurried");
                }
                */
                if(thirst >= attraction && thirst >= tiredness)
                {
                    // Go get a drink
                    currentAction = PedestrianAction.GoToVendingMachine;
                    //Debug.Log("drinky");
                }
                else if(attraction >= thirst && attraction >= tiredness)
                {
                    // Go watch entertainers
                    currentAction = PedestrianAction.Idle;
                    Debug.Log("attracted");

                }
                else
                {
                    // Find a bench
                    currentAction = PedestrianAction.Idle;
                    Debug.Log("tired");
                }            
            }
        }
        else
        {

        }


        


    }


}
