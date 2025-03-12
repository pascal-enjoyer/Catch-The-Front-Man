using System.Collections;
using UnityEngine;

public abstract class Bonus : MonoBehaviour
{
    [SerializeField] protected float duration = 5f;

    public Sprite bonusIcon;

    protected GameObject target;

    private float timer;

    public float TimeToEnd
    {
        get { return timer; }
    }

    public float Duration
    {
        get { return duration; }
    }

    public void Initialize(GameObject target)
    {
        this.target = target;
        timer = duration;
        ApplyEffect(true);
        StartCoroutine(BuffTimer());
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