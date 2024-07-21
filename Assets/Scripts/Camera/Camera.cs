using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour
{
    public Transform target;
    public float height = 10f;
    public float distance = 5f;
    public CinemachineVirtualCamera virtualCamera;

    public List<Transform> AerialViews;

    private int currentAerialIndex = 0;

    public List<Transform> DoorwayViews;

    private int currentDoorwayIndex = 0;



    void Start()
    {
        if (target != null)
        {
            SetCameraTarget(target, new Vector3(0, 3, -5));
        }
    }

    void Update()
    {
        if (target == null && PedestrianManager.instance.pedestrians.Count > 0)
        {
            target = PedestrianManager.instance.getRandomPedestrian();
            SetCameraTarget(target, new Vector3(0, 3, -5));
        }
    }

    public void switchPedestrian()
    {
        target = PedestrianManager.instance.getRandomPedestrian();
        SetCameraTarget(target, new Vector3(0, 3, -5));
    }

    public void getAerialView()
    {
        currentAerialIndex = (currentAerialIndex + 1) % AerialViews.Count;
        SetCameraTransform(AerialViews[currentAerialIndex]);
    }

    public void getDoorwayView()
    {
        currentDoorwayIndex = (currentDoorwayIndex + 1) % DoorwayViews.Count;
        SetCameraTransform(DoorwayViews[currentDoorwayIndex]);
    }

    void SetCameraTarget(Transform newTarget, Vector3 offset)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = newTarget;
            virtualCamera.LookAt = newTarget;

            // Adjust the virtual camera's settings to maintain the desired height and distance
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = offset;
            }
        }
    }

    void SetCameraTransform(Transform viewTransform)
    {
        if (virtualCamera != null)
        {
            virtualCamera.LookAt = null;
            virtualCamera.Follow = null;

            virtualCamera.transform.position = viewTransform.position;
            virtualCamera.transform.rotation = viewTransform.rotation;
        }
        else
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
    }
}
