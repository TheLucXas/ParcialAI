using UnityEngine;

public static class SteeringUtils
{
    public static Vector3 CalculateSeek(Vector3 currentPosition, Vector3 velocity, Vector3 targetPosition, float maxSpeed, float maxForce)
    {
        Vector3 desired = (targetPosition - currentPosition).normalized * maxSpeed;
        return CalculateSteering(velocity, desired, maxForce);
    }

    public static Vector3 CalculateFlee(Vector3 currentPosition, Vector3 velocity, Vector3 targetPosition, float maxSpeed, float maxForce)
    {
        Vector3 desired = (targetPosition - currentPosition).normalized * maxSpeed;
        return CalculateSteering(velocity, -desired, maxForce);
    }

    public static Vector3 CalculateArrive(Vector3 currentPosition, Vector3 velocity, Vector3 targetPosition, float maxSpeed, float maxForce, float arriveRadius)
    {
        Vector3 dir = targetPosition - currentPosition;
        float distance = dir.magnitude;
        float speed = maxSpeed;

        if (distance <= arriveRadius)
        {
            speed *= distance / arriveRadius;
        }

        Vector3 desired = dir.normalized * speed;
        return CalculateSteering(velocity, desired, maxForce);
    }

    public static Vector3 GetFuturePosition(Vector3 currentPosition, float maxSpeed, Vector3 targetPosition, Vector3 targetVelocity)
    {
        float distanceToTarget = (targetPosition - currentPosition).magnitude;
        float predictedTime = distanceToTarget / (maxSpeed + targetVelocity.magnitude);
        return targetPosition + targetVelocity * predictedTime;
    }

    public static Vector3 CalculatePursuit(Vector3 currentPosition, Vector3 velocity, float maxSpeed, float maxForce, Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 futurePos = GetFuturePosition(currentPosition, maxSpeed, targetPosition, targetVelocity);
        return CalculateSeek(currentPosition, velocity, futurePos, maxSpeed, maxForce);
    }

    public static Vector3 CalculateEvade(Vector3 currentPosition, Vector3 velocity, float maxSpeed, float maxForce, Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 futurePos = GetFuturePosition(currentPosition, maxSpeed, targetPosition, targetVelocity);
        return CalculateFlee(currentPosition, velocity, futurePos, maxSpeed, maxForce);
    }

    public static Vector3 CalculateSteering(Vector3 velocity, Vector3 desired, float maxForce)
    {
        Vector3 steering = desired - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering * Time.deltaTime;
    }
}