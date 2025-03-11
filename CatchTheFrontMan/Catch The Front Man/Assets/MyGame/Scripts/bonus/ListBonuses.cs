using UnityEngine;

public class HealthBonus : IBonusEffect
{
    private float healthAmount;

    public HealthBonus(float amount)
    {
        healthAmount = amount;
    }

    public void ApplyEffect(PlayerController player)
    {
    }
}

public class SpeedBonus : IBonusEffect
{
    private float speedAmount;
    private float duration;

    public SpeedBonus(float amount, float duration)
    {
        speedAmount = amount;
        this.duration = duration;
    }

    public void ApplyEffect(PlayerController player)
    {
    }
}