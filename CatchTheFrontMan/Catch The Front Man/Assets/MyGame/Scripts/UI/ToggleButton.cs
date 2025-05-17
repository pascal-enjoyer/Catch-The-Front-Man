using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnToggleStart;
    public UnityEvent OnToggleEnd;


    // Вызывается при нажатии кнопки
    public void OnPointerDown(PointerEventData eventData)
    {
          OnToggleStart?.Invoke();
        Debug.Log("Кнопка зажата");
    }

    // Вызывается при отпускании кнопки
    public void OnPointerUp(PointerEventData eventData)
    {
        OnToggleEnd?.Invoke();
        Debug.Log("Кнопка отжата");
    }
}