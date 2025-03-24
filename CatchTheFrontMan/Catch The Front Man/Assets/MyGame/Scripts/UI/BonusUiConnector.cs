using UnityEngine;

public class BonusUiConnector : MonoBehaviour
{
    public BonusUIManager bonusUIManager;
    public PlayerBonusHandler playerBonusHandler;


    private void Awake()
    {
        PlayerManager.PlayerChanged.AddListener(Setup);

    }


    private void Setup(GameObject newPlayer)
    {
        playerBonusHandler = newPlayer.GetComponent<PlayerBonusHandler>();
        playerBonusHandler.BonusActivated.AddListener(bonusUIManager.SpawnBonusUI);
    }

}
