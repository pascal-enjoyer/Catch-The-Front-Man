using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnToggleStart;
    public UnityEvent OnToggleEnd;


    // ���������� ��� ������� ������
    public void OnPointerDown(PointerEventData eventData)
    {
          OnToggleStart?.Invoke();
        Debug.Log("������ ������");
    }

    // ���������� ��� ���������� ������
    public void OnPointerUp(PointerEventData eventData)
    {
        OnToggleEnd?.Invoke();
        Debug.Log("������ ������");
    }
}