using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Train : MonoBehaviour
{
    public float timeTillDeparture;
    public float remainingTimeTillDeparture;
    private TrainManager trainManager;

    public Vector3 position;

    void Start()
    {
        trainManager = FindObjectOfType<TrainManager>();
        remainingTimeTillDeparture = timeTillDeparture;
        position = transform.position;
        StartCoroutine(DepartureRoutine());
    }

    void Update()
    {
        UpdateRemainingTimeTillDeparture();
    }

    private IEnumerator DepartureRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(remainingTimeTillDeparture);
            Depart();
        }
    }

    private void Depart()
    {
        Debug.Log($"{gameObject.name} is departing.");
        timeTillDeparture = trainManager.GetNextDepartureTime();
        remainingTimeTillDeparture = timeTillDeparture; // Reset the timer
        StartCoroutine(DepartureRoutine());
    }

    private void UpdateRemainingTimeTillDeparture()
    {
        if (remainingTimeTillDeparture > 0)
        {
            remainingTimeTillDeparture -= Time.deltaTime;
        }
    }

    public float GetRemainingTimeTillDeparture()
    {
        return Mathf.Max(remainingTimeTillDeparture, 0); // Ensuring it doesn't return negative time
    }
}
