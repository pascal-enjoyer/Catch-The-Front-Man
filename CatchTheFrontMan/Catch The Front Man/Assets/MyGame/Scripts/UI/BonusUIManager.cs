using UnityEngine;
using UnityEngine.UI;

public class BonusUIManager : MonoBehaviour
{
    public GameObject bonusElementPrefab; // Префаб элемента UI для бонуса
    public Transform bonusContainer; // Контейнер для бонусов



    public void SpawnBonusUI(Bonus bonus)
    {
        if (bonusElementPrefab == null || bonusContainer == null)
        {
            Debug.LogError("BonusElementPrefab or BonusContainer is not assigned!");
            return;
        }

        // Создаем новый элемент UI
        GameObject bonusElement = Instantiate(bonusElementPrefab, bonusContainer);

        // Настраиваем RectTransform
        RectTransform rectTransform = bonusElement.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f); // Min X: 0, Min Y: 0.5
            rectTransform.anchorMax = new Vector2(1, 0.5f); // Max X: 1, Max Y: 0.5
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // Pivot: Center
            rectTransform.sizeDelta = new Vector2(0, 50); // Height: 50, Width: 0 (растягивается)
        }

        // Инициализация элемента
        BonusUI bonusUI = bonusElement.GetComponent<BonusUI>();
        if (bonusUI != null)
        {
            bonusUI.Init(bonus);
        }
        else
        {
            Debug.LogError("BonusUI component is missing on the prefab!");
            Destroy(bonusElement);
        }
    }
}