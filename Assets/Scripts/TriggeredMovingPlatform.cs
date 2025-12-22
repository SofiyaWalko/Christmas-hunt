using UnityEngine;

public class TriggeredMovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    [HideInInspector] public bool isActive = false;

    private Vector3 worldPointA;
    private Vector3 worldPointB;
    private Vector3 targetPosition;
    private bool movingToB = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Point A or B is not assigned on " + gameObject.name);
            enabled = false;
            return;
        }

        worldPointA = pointA.position;
        worldPointB = pointB.position;
        movingToB = true;
        targetPosition = worldPointB;
    }

    void Update()
    {
        if (!isActive) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            movingToB = !movingToB;
            targetPosition = movingToB ? worldPointB : worldPointA;
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
