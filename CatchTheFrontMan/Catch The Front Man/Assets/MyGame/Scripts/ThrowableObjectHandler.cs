using UnityEngine;

public class ThrowableObjectHandler : MonoBehaviour
{
    private ObjectsThrowZone _throwZone;
    private bool _isHighlighted;
    

    public void Setup(ObjectsThrowZone zone)
    {
        _throwZone = zone;



        // ��������, ��� ������ ����� ���������
        if (!TryGetComponent<Collider>(out var collider))
        {
            Debug.LogWarning($"No Collider found on {name}. Adding BoxCollider.", this);
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }

        // �������� ��������� ����� ��� ����� � ����
        Highlight(true);
    }

    public void OnClick()
    {
        if (_throwZone != null)
        {
            _throwZone.ThrowObject(gameObject);
        }
        else
        {
            Debug.LogError($"OnClick failed: throwZone is null on {name}");
        }
    }

    public void Highlight(bool enable)
    {

        if (enable && !_isHighlighted)
        {
            _isHighlighted = true;
            Debug.Log($"Highlight enabled on {name}");
        }
        else if (!enable && _isHighlighted)
        {
            _isHighlighted = false;
            Debug.Log($"Highlight disabled on {name}");
        }
    }

}