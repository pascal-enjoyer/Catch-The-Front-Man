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

        // �������������� DialogUI � DialogSystem
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.Initialize(dialogUI);
        }
        else
        {
            Debug.LogError("DialogManager: DialogSystem.Instance is null.", this);
        }

        // ��������, ��� DialogUI ���������� ��������
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

        // ���������� DialogUI
        dialogUI.gameObject.SetActive(true);

        // ��������� ������ ����� DialogSystem
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