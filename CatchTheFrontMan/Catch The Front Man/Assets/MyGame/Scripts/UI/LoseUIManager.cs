using UnityEngine;

public class LoseUIManager : MonoBehaviour
{
    public GameObject LoseUIprefab;

    public GameObject spawnedLoseUI;

    public PlayerReviver PlayerReviver;

    private void Awake()
    {
        PlayerManager.PlayerChanged.AddListener(SetupLoseUI);
    }

    private void SetupLoseUI(GameObject newPlayer)
    {

        newPlayer.GetComponent<PlayerController>().PlayerDie.AddListener(SpawnLoseUI);

    }

    public void SpawnLoseUI()
    {
        if (spawnedLoseUI == null)
        {
            spawnedLoseUI = Instantiate(LoseUIprefab, transform);
            spawnedLoseUI.GetComponent<LoseUI>().PlayerRevived.AddListener(PlayerReviver.RevivePlayer);
            spawnedLoseUI.GetComponent<LoseUI>().PlayerRevived.AddListener(DestroyLoseUI);
        }
    }

    public void DestroyLoseUI()
    {
        Destroy(spawnedLoseUI);
    }
}
