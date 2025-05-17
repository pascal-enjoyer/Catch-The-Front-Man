using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private DialogUI dialogUI;

    private void Start()
    {
        if (dialogUI == null)
        {
            Debug.LogError("DialogManager: dialogUI is not assigned.", this);
            return;
        }

        // Инициализируем DialogUI в DialogSystem
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.Initialize(dialogUI);
        }
        else
        {
            Debug.LogError("DialogManager: DialogSystem.Instance is null.", this);
        }

        // Убедимся, что DialogUI изначально выключен
        dialogUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogEnded += OnDialogEnded;
        }
    }

    private void OnDisable()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogEnded -= OnDialogEnded;
        }
    }

    public void StartDialog(DialogData dialogData)
    {
        if (dialogUI == null)
        {
            Debug.LogError("DialogManager: dialogUI is not assigned.", this);
            return;
        }

        // Активируем DialogUI
        dialogUI.gameObject.SetActive(true);

        // Запускаем диалог через DialogSystem
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(dialogData);
        }
        else
        {
            Debug.LogError("DialogManager: DialogSystem.Instance is null during StartDialog.", this);
        }
    }

    private void OnDialogEnded()
    {
        if (dialogUI != null)
        {
            dialogUI.gameObject.SetActive(false);
        }
    }
}