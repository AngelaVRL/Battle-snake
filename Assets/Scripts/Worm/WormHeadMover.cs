using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHeadMover : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 220f;

    public Vector2 Direction { get; private set; } = Vector2.right;

    public void Step(Vector2 desiredDirection, float deltaTime)
    {
        if (desiredDirection.sqrMagnitude > 0.0001f)
        {
            float currentAngle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * deltaTime);
            float radians = newAngle * Mathf.Deg2Rad;
            Direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        transform.position += (Vector3)(Direction * speed * deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg);
    }
}