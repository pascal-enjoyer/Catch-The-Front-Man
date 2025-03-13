// BonusData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Bonuses/New Bonus")]
public class BonusData : ScriptableObject
{
    [Header("Visuals")]
    public Sprite icon;
    public Color color = Color.white;

    [Header("Settings")]
    public float duration = 5f;

    [Header("Prefab")]
    public GameObject bonusPrefab; // Шаблон бонуса
}