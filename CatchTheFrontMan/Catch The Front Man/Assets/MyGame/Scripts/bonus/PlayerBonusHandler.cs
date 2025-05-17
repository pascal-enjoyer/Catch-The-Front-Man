using UnityEngine;
using UnityEngine.Events;

public class PlayerBonusHandler : MonoBehaviour
{
    public UnityEvent<Bonus> BonusActivated;

    public void AcquireBuff(GameObject buffPrefab)
    {
        var prefabBonus = buffPrefab.GetComponent<Bonus>();
        if (prefabBonus == null)
        {
            Debug.LogError("Prefab has no Bonus component!");
            return;
        }

        // Создаем экземпляр бонуса
        var buffInstance = gameObject.AddComponent(prefabBonus.GetType()) as Bonus;

        // Копируем данные из префаба
        buffInstance.CopyFrom(prefabBonus);

        // Инициализируем
        buffInstance.Initialize(gameObject);

        BonusActivated?.Invoke(buffInstance);
    }
}