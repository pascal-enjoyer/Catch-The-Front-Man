using UnityEngine;
using UnityEngine.Events;

public class UIPanelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab; // Префаб кнопки
    private GameObject currentButtonPrefab; // Текущая созданная кнопка

    [Header("Button Actions")]
    [SerializeField] private UnityEvent onPointerDownActions; // События для нажатия кнопки
    [SerializeField] private UnityEvent onPointerUpActions; // События для отпускания кнопки

    public void SpawnButton()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("UIPanelSpawner: Button prefab not assigned!");
            return;
        }

        // Создаём кнопку
        currentButtonPrefab = Instantiate(buttonPrefab, transform);

    }

    public void AddListenersToButton()
    {
        ToggleButton toggleButton = currentButtonPrefab.GetComponent<ToggleButton>();

        if (toggleButton == null)
        {
            Debug.LogError("UIPanelSpawner: ToggleButton component not found on button prefab!");
            Destroy(currentButtonPrefab);
            currentButtonPrefab = null;
            return;
        }

        // Подписываем события из инспектора на события ToggleButton
        toggleButton.OnToggleStart.AddListener(() => onPointerDownActions.Invoke());
        toggleButton.OnToggleEnd.AddListener(() => onPointerUpActions.Invoke());

        Debug.Log("Button spawned and actions assigned.");
    }

    public void DestroyButton()
    {
        if (currentButtonPrefab == null)
        {
            Debug.LogWarning("UIPanelSpawner: No button to destroy.");
            return;
        }

        // Очищаем подписки
        ToggleButton toggleButton = currentButtonPrefab.GetComponent<ToggleButton>();
        if (toggleButton != null)
        {
            toggleButton.OnToggleStart.RemoveAllListeners();
            toggleButton.OnToggleEnd.RemoveAllListeners();
        }

        // Уничтожаем кнопку
        Destroy(currentButtonPrefab);
        currentButtonPrefab = null;
        Debug.Log("Button destroyed.");
    }
}