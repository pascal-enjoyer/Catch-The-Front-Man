using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IDialogUI
{
    void ShowDialog(string text, List<DialogOption> options, DialogNode currentNode);
    void HideDialog();
    void SetDialogData(DialogData dialogData);
}

public class DialogUI : MonoBehaviour, IDialogUI
{
    [SerializeField] private Text dialogText;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private Image playerAvatarImage; // UI элемент для аватарки игрока
    [SerializeField] private Image interlocutorAvatarImage; // UI элемент для аватарки собеседника

    private List<Button> optionButtons = new List<Button>();
    private bool isTypingComplete = false;
    private List<DialogOption> currentOptions;
    private DialogData currentDialogData;

    private void OnEnable()
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.OnTypewriterComplete += OnTypewriterComplete;
        }
    }

    private void OnDisable()
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.OnTypewriterComplete -= OnTypewriterComplete;
        }
    }

    public void ShowDialog(string text, List<DialogOption> options, DialogNode currentNode)
    {
        isTypingComplete = false;
        currentOptions = options;
        ClearOptions();

        // Обновляем аватарки
        if (playerAvatarImage != null)
        {
            playerAvatarImage.sprite = currentNode?.PlayerAvatar;
            playerAvatarImage.enabled = currentNode?.PlayerAvatar != null;
        }
        else
        {
            Debug.LogWarning("DialogUI: playerAvatarImage is null.", this);
        }

        if (interlocutorAvatarImage != null)
        {
            interlocutorAvatarImage.sprite = currentNode?.InterlocutorAvatar;
            interlocutorAvatarImage.enabled = currentNode?.InterlocutorAvatar != null;
        }
        else
        {
            Debug.LogWarning("DialogUI: interlocutorAvatarImage is null.", this);
        }

        if (typewriterEffect != null)
        {
            typewriterEffect.StartTyping(text);
        }
        else
        {
            dialogText.text = text;
            isTypingComplete = true;
            DisplayOptions();
        }

        gameObject.SetActive(true);
    }

    public void HideDialog()
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }
        gameObject.SetActive(false);
        ClearOptions();
        if (playerAvatarImage != null) playerAvatarImage.enabled = false;
        if (interlocutorAvatarImage != null) interlocutorAvatarImage.enabled = false;
    }

    public void SetDialogData(DialogData dialogData)
    {
        currentDialogData = dialogData;
    }

    private void ClearOptions()
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
        }
        optionButtons.Clear();
    }

    private void OnOptionSelected(int index)
    {
        if (!isTypingComplete)
        {
            if (typewriterEffect != null)
            {
                typewriterEffect.SkipTyping(currentOptions[index].Text);
            }
            return;
        }

        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.SelectOption(index);
        }
        else
        {
            Debug.LogError("DialogUI: DialogSystem.Instance is null during OnOptionSelected.", this);
        }
    }

    private void OnTypewriterComplete()
    {
        isTypingComplete = true;
        DisplayOptions();
    }

    private void DisplayOptions()
    {
        ClearOptions();
        for (int i = 0; i < currentOptions.Count; i++)
        {
            int index = i;
            var button = Instantiate(optionButtonPrefab, optionsContainer);
            var buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = currentOptions[i].Text;
            }
            else
            {
                Debug.LogWarning("DialogUI: Button prefab is missing TextMeshProUGUI component.", this);
            }
            button.onClick.AddListener(() => OnOptionSelected(index));
            optionButtons.Add(button);
        }
    }
}