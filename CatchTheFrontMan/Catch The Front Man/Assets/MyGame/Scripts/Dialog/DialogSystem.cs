using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogSystem
{
    void StartDialog(DialogData dialogData);
    void SelectOption(int optionIndex);
    void EndDialog();
    event Action OnDialogEnded;
}

public class DialogSystem : MonoBehaviour, IDialogSystem
{
    public static DialogSystem Instance { get; private set; }

    public event Action OnDialogEnded;

    private DialogData currentDialog;
    private DialogNode currentNode;
    private IDialogUI dialogUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(IDialogUI dialogUI)
    {
        this.dialogUI = dialogUI;
        if (dialogUI == null)
        {
            Debug.LogError("DialogSystem: dialogUI is null during initialization.", this);
        }
    }

    public void StartDialog(DialogData dialogData)
    {
        if (dialogData == null)
        {
            Debug.LogError("DialogSystem: dialogData is null.", this);
            return;
        }
        if (dialogUI == null)
        {
            Debug.LogError("DialogSystem: dialogUI is not initialized.", this);
            return;
        }

        currentDialog = dialogData;
        currentNode = dialogData.Nodes.Find(node => node.NodeId == dialogData.StartingNodeId);
        if (currentNode == null)
        {
            Debug.LogError($"DialogSystem: Starting node '{dialogData.StartingNodeId}' not found.", this);
            return;
        }

        dialogUI.SetDialogData(dialogData);
        dialogUI.ShowDialog(currentNode.Text, currentNode.Options, currentNode);
        currentNode.OnNodeEnter?.Invoke();
        EventManager.RaiseDialogStarted();
    }

    public void SelectOption(int optionIndex)
    {
        if (currentNode == null || optionIndex < 0 || optionIndex >= currentNode.Options.Count)
        {
            Debug.LogError("DialogSystem: Invalid option index or currentNode is null.", this);
            return;
        }

        var option = currentNode.Options[optionIndex];
        option.OnSelect?.Invoke();
        currentNode = currentDialog.Nodes.Find(node => node.NodeId == option.NextNodeId);
        if (currentNode != null)
        {
            dialogUI.ShowDialog(currentNode.Text, currentNode.Options, currentNode);
            currentNode.OnNodeEnter?.Invoke();
        }
        else
        {
            EndDialog();
        }
    }

    public void EndDialog()
    {
        if (dialogUI == null)
        {
            Debug.LogError("DialogSystem: dialogUI is null during EndDialog.", this);
            return;
        }

        dialogUI.HideDialog();
        OnDialogEnded?.Invoke();
        EventManager.RaiseDialogEnded();
    }
}