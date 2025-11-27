using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Transform target;

    void Start()
    {
        target = pointB; // всегда начнёт с точки B
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // проверка достижения
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            // смена цели
            if (target == pointA)
                target = pointB;
            else
                target = pointA;
        }
    }
}
