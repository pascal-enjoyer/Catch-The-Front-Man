using UnityEngine;

public interface IBonusEffect
{
    void ApplyEffect(PlayerController player);
}

public class Bonus : MonoBehaviour
{
    public IBonusEffect effect; // Эффект бонуса

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyEffect(effect); // Применяем эффект
                Destroy(gameObject); // Уничтожаем бонус
            }
        }
    }
}