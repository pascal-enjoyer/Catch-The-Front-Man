using UnityEngine;
using System.Collections;

public abstract class Bonus : MonoBehaviour
{
    private BonusData data;
    protected GameObject target;
    private float timer;

    public BonusData Data { get { return data; } }
    public float TimeToEnd { get { return timer; } }

    public void SetData(BonusData data)
    {
        this.data = data;
    }

    public void Initialize(GameObject target)
    {
        this.target = target;
        timer = data.duration;

        ApplyEffect(true);
        StartCoroutine(BuffTimer());

        // Для теста - можно удалить
        Debug.Log($"Bonus initialized: {data.name} | Duration: {data.duration}");
    }

    private IEnumerator BuffTimer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateEffect();
            yield return null;
        }
        ApplyEffect(false);
        Destroy(this);
    }

    protected abstract void ApplyEffect(bool activate);
    protected virtual void UpdateEffect() { }
}