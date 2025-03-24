using UnityEngine;

public static class PredictionCalculator
{
    public static Vector3 CalculatePredictedPosition(Vector3 shooterPosition,
                                                    Vector3 targetPosition,
                                                    Vector3 targetVelocity,
                                                    float projectileSpeed)
    {
        if (projectileSpeed <= 0) return targetPosition;

        Vector3 direction = targetPosition - shooterPosition;
        float distance = direction.magnitude;
        float timeToTarget = distance / projectileSpeed;

        // Первое приближение
        Vector3 predictedPosition = targetPosition + targetVelocity * timeToTarget;

        // Уточнение позиции с учетом нового расстояния
        Vector3 predictedDirection = predictedPosition - shooterPosition;
        float predictedDistance = predictedDirection.magnitude;
        timeToTarget = predictedDistance / projectileSpeed;

        return targetPosition + targetVelocity * timeToTarget;
    }
}