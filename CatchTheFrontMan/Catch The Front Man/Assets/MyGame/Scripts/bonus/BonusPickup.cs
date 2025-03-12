using UnityEngine;

public class BonusPickup : MonoBehaviour
{
    [SerializeField] private GameObject buffPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() && !other.gameObject.GetComponent<PlayerController>().isDead)
        {
            other.GetComponent<PlayerBonusHandler>().AcquireBuff(buffPrefab);
            Destroy(gameObject);
        }
    }
}