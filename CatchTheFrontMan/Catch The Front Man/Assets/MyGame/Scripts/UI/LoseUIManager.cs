using UnityEngine;

public class LoseUIManager : MonoBehaviour
{
    public GameObject LoseUIprefab;

    public GameObject spawnedLoseUI;

    public void SpawnLoseUI()
    {
        LoseUIprefab.SetActive(true);
        LoseUIprefab.GetComponent<LoseUI>().PlayerRevived.AddListener(DestroyLoseUI);
    }

    public void DestroyLoseUI()
    {
        Destroy(spawnedLoseUI);
    }
}
