using UnityEngine;

public class LoseUIManager : MonoBehaviour
{
    public GameObject LoseUIprefab;

    public GameObject spawnedLoseUI;

    public PlayerReviver PlayerReviver;

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
