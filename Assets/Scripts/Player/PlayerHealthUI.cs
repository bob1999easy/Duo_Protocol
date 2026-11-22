using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text healthText;

    public void SetHealth(int current, int max)
    {
        if (fillImage != null && max > 0)
            fillImage.fillAmount = (float)current / max;

        if (healthText != null)
            healthText.text = current + " / " + max;
    }
}