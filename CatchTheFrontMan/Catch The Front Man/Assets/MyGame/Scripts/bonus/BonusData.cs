using UnityEngine;

[CreateAssetMenu(menuName = "Bonuses/Bonus Data")]
public class BonusData : ScriptableObject
{
    public Sprite icon;
    public float duration;
    public GameObject effectPrefab; // Префаб с логикой бонуса (например, FreezeEnemiesBonus)
}