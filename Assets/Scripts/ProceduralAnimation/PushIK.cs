using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class PushIK : MonoBehaviour
{
    Commuter commuter;
    Exiter exiter;

    public TwoBoneIKConstraint twoBoneIK; 
    public Transform target; 
    public float lerpSpeed = 15f;
    

    void Start()
    {
        commuter = GetComponent<Commuter>();
        exiter = GetComponent<Exiter>();
    }

    void Update()
    {
        if (commuter.enabled == true && commuter.inDoorwayRegion && commuter.door != null)
        {
            float distanceToDoor = Vector3.Distance(transform.position, commuter.door.transform.position);
            if (distanceToDoor <= 1.5f)
            {
                // Enable IK
                if(commuter.isHolder)
                {
                    twoBoneIK.weight = Mathf.Lerp(twoBoneIK.weight, 1f, Time.deltaTime * lerpSpeed);
                }
                
                if(commuter.onPushSide)
                {
                    target.position = commuter.door.pushTarget.position;
                    target.rotation = commuter.door.pushTarget.rotation;
                }
                else
                {
                    target.position = commuter.door.pullTarget.position;
                    target.rotation = commuter.door.pullTarget.rotation;
                }
                
            }
            else
            {
                // Disable IK
                twoBoneIK.weight = Mathf.Lerp(twoBoneIK.weight, 0f, Time.deltaTime * lerpSpeed);
            }
        }
        if (exiter.enabled == true && exiter.inDoorwayRegion && exiter.door != null)
        {
            float distanceToDoor = Vector3.Distance(transform.position, exiter.door.transform.position);
            if (distanceToDoor <= 1.5f)
            {
                // Enable  IK
                if(exiter.isHolder)
                {
                    twoBoneIK.weight = Mathf.Lerp(twoBoneIK.weight, 1f, Time.deltaTime * lerpSpeed);
                }
                if(exiter.onPushSide)
                {
                    target.position = exiter.door.pushTarget.position;
                    target.rotation = exiter.door.pushTarget.rotation;
                }
                else
                {
                    target.position = exiter.door.pullTarget.position;
                    target.rotation = exiter.door.pullTarget.rotation;
                }
            }
            else
            {
                // Disable IK
                twoBoneIK.weight = Mathf.Lerp(twoBoneIK.weight, 0f, Time.deltaTime * lerpSpeed);
            }
        }
        else
        {
            twoBoneIK.weight = Mathf.Lerp(twoBoneIK.weight, 0f, Time.deltaTime * lerpSpeed);
        }
    }
}
