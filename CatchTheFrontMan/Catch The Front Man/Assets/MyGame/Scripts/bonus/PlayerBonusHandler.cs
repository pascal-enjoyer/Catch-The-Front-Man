using UnityEngine;
using UnityEngine.Events;

public class PlayerBonusHandler : MonoBehaviour
{
    public UnityEvent<Bonus> BonusActivated;

    public void AcquireBuff(GameObject buffPrefab)
    {
        var buffInstance = gameObject.AddComponent(buffPrefab.GetComponent<Bonus>().GetType());
        Bonus buffComponent = buffInstance as Bonus;
        buffComponent.Initialize(gameObject);
        BonusActivated?.Invoke(buffComponent);
    }
    
}