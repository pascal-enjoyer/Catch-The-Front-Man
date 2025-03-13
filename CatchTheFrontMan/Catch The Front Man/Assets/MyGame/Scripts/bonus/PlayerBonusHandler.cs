using UnityEngine;
using UnityEngine.Events;

public class PlayerBonusHandler : MonoBehaviour
{
    public UnityEvent<Bonus> BonusActivated;

    public void AcquireBuff(GameObject buffPrefab)
    {
        // Получаем компонент Bonus из префаба
        var prefabBonus = buffPrefab.GetComponent<Bonus>();
        if (prefabBonus == null)
        {
            Debug.LogError("Prefab has no Bonus component!");
            return;
        }

        // Создаем новый компонент на игроке
        var buffInstance = gameObject.AddComponent(prefabBonus.GetType()) as Bonus;

        // Копируем данные из префаба в новый компонент
        buffInstance.SetData(prefabBonus.Data);

        // Инициализируем бонус
        buffInstance.Initialize(gameObject);

        // Вызываем событие
        BonusActivated?.Invoke(buffInstance);
    }
}