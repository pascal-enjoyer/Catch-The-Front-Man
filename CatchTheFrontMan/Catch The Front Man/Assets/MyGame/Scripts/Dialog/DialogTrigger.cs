using UnityEditor.SceneManagement;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private DialogData dialogData;
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private Transform dialogCameraPosition; // ������� ������ ��� �������

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogManager == null)
            {
                Debug.LogError("DialogTrigger: dialogManager is not assigned.", this);
                return;
            }
            if (dialogData == null)
            {
                Debug.LogError("DialogTrigger: dialogData is null. Assign DialogData in Inspector.", this);
                return;
            }
            dialogManager.StartDialog(dialogData);
            // ���������� ������ � �������� �������
            if (dialogCameraPosition != null)
            {
                CameraMovement.Instance.MoveToDialogPosition(dialogCameraPosition);
                EventManager.OnDialogEnded += CameraMovement.Instance.OnDialogEnd;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DialogSystem.Instance == null)
            {
                Debug.LogError("DialogTrigger: DialogSystem.Instance is null on exit.", this);
                return;
            }
            DialogSystem.Instance.EndDialog();
            // ���������� ������ � ����� ����������
            CameraMovement.Instance.OnDialogEnd();
            Destroy(this);
        }
    }


}