using UnityEngine;

public class TriggeredMovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    [HideInInspector] public bool isActive = false;

    private Transform target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (!isActive) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            target = (target == pointA) ? pointB : pointA;
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
