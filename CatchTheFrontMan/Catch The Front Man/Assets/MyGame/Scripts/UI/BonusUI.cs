
using UnityEngine;
using UnityEngine.UI;

public class BonusUI : MonoBehaviour
{
    public Text timer;
    public Image fillingImage;
    public Image icon;
    public bool isCounting = false;
    public Bonus currentBonus;

    public void Update()
    {
        if (!isCounting) { return; }
        if (currentBonus == null)
            Destroy(gameObject);
        else
            UpdateTimer();
        
    }

    public void Init(Bonus activatedBonus)
    {
        currentBonus = activatedBonus;
        icon.sprite = currentBonus.Data.icon;

        isCounting = true;
    }

    public void UpdateTimer()
    {
        timer.text = currentBonus.TimeToEnd.ToString().Substring(0, 3);
        fillingImage.fillAmount = currentBonus.TimeToEnd/currentBonus.Data.duration;
    }

}
