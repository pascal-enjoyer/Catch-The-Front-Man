using UnityEngine;

public class BonusUiConnector : MonoBehaviour
{
    public BonusUIManager bonusUIManager;
    public PlayerBonusHandler playerBonusHandler;

    private void Start()
    {
        playerBonusHandler.BonusActivated.AddListener(bonusUIManager.SpawnBonusUI);
    }
}
