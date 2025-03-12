using UnityEngine;

public class BonusUIManager : MonoBehaviour
{
    public GameObject bonusElementPrefab;

    public GameObject currentBonus;

    

    public void SpawnBonusUI(Bonus bonus)
    {
        currentBonus = Instantiate(bonusElementPrefab, transform);
        currentBonus.GetComponent<BonusUI>().Init(bonus);
    }
}
