using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MovementButtonsController : MonoBehaviour
{
    public PlayerController playerController => PlayerManager.Instance.currentPlayer.GetComponent<PlayerController>();

    public Button rightButton;
    public Button leftButton;
    public Button downButton;
    public Button upButton;

    private List<Button> buttons;
    private List<UnityAction> clickActions = new List<UnityAction>();

    public bool isVisibleOnStart = false;

    private void Start()
    {
        buttons = new List<Button> { rightButton, leftButton, downButton, upButton };
        clickActions = new List<UnityAction> { OnRightButtonPressed, OnLeftButtonPressed, OnDownButtonPressed, OnUpButtonPressed };
        ToggleControlButtons(isVisibleOnStart);

        // Add pointer down and up listeners for hold detection
        AddHoldListeners();
    }

    private void AddHoldListeners()
    {
        foreach (var button in buttons)
        {
            var trigger = button.gameObject.AddComponent<EventTrigger>();

            // Pointer down
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => OnButtonPointerDown(button));
            trigger.triggers.Add(pointerDown);

            // Pointer up
            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => OnButtonPointerUp(button));
            trigger.triggers.Add(pointerUp);
        }
    }

    public void ToggleControlButtons(bool isOn)
    {
        for (int i = 0; i < clickActions.Count; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(clickActions[i]);
                buttons[i].gameObject.SetActive(isOn);
            }
        }
    }

    public void OnLeftButtonPressed()
    {
        playerController.OnLeftButtonClicked();
    }

    public void OnRightButtonPressed()
    {
        playerController.OnRightButtonClicked();
    }

    public void OnDownButtonPressed()
    {
        playerController.OnDownButtonClicked();
    }

    public void OnUpButtonPressed()
    {
        playerController.OnUpButtonClicked();
    }

    private void OnButtonPointerDown(Button button)
    {
        if (button == leftButton)
            playerController.OnLeftButtonHeld(true);
        else if (button == rightButton)
            playerController.OnRightButtonHeld(true);
        else if (button == downButton)
            playerController.OnDownButtonHeld(true);
    }

    private void OnButtonPointerUp(Button button)
    {
        if (button == leftButton)
            playerController.OnLeftButtonHeld(false);
        else if (button == rightButton)
            playerController.OnRightButtonHeld(false);
        else if (button == downButton)
            playerController.OnDownButtonHeld(false);
    }
}