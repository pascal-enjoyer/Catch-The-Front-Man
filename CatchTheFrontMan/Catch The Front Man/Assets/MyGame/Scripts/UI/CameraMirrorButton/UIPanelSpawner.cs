using UnityEngine;
using UnityEngine.Events;

public class UIPanelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab; // ������ ������
    private GameObject currentButtonPrefab; // ������� ��������� ������

    [Header("Button Actions")]
    [SerializeField] private UnityEvent onPointerDownActions; // ������� ��� ������� ������
    [SerializeField] private UnityEvent onPointerUpActions; // ������� ��� ���������� ������

    public void SpawnButton()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("UIPanelSpawner: Button prefab not assigned!");
            return;
        }

        // ������ ������
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

        // ����������� ������� �� ���������� �� ������� ToggleButton
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

        // ������� ��������
        ToggleButton toggleButton = currentButtonPrefab.GetComponent<ToggleButton>();
        if (toggleButton != null)
        {
            toggleButton.OnToggleStart.RemoveAllListeners();
            toggleButton.OnToggleEnd.RemoveAllListeners();
        }

        // ���������� ������
        Destroy(currentButtonPrefab);
        currentButtonPrefab = null;
        Debug.Log("Button destroyed.");
    }
}