using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class FillableObject : MonoBehaviour
{
    [SerializeField] private Image fillImage; // UI Image to control

    void Awake()
    {
        // Validate setup
        if (fillImage == null)
            Debug.LogError($"Fill Image not assigned on {name}!", this);
        else
        {
            // Ensure initial setup
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Top;
            fillImage.fillAmount = 1f;
        }
    }

    public void SetFillAmount(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
            Debug.Log($"FillableObject on {name} set fill amount to {amount}");
        }
        else
            Debug.LogError($"Cannot set fill amount on {name}: Fill Image is null!");
    }

    public void SetFillColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
            Debug.Log($"FillableObject on {name} set color to {color}");
        }
        else
            Debug.LogError($"Cannot set fill color on {name}: Fill Image is null!");
    }

    public void SetFillMethod(Image.FillMethod method, Image.OriginVertical origin)
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = method;
            fillImage.fillOrigin = (int)origin;
            Debug.Log($"FillableObject on {name} set fill method to {method}, origin {origin}");
        }
        else
            Debug.LogError($"Cannot set fill method on {name}: Fill Image is null!");
    }
}