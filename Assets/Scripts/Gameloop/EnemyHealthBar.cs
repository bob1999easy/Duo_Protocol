using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Image fillImage;

    private Camera mainCam;

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(int current, int max)
    {
        if (fillImage != null && max > 0)
            fillImage.fillAmount = (float)current / max;
    }

    private void LateUpdate()
    {
        // find the local player's camera (only the owner's camera is enabled)
        if (mainCam == null || !mainCam.isActiveAndEnabled)
            mainCam = Camera.main;

        if (mainCam == null)
            return;

        // billboard: always face the camera
        transform.rotation = mainCam.transform.rotation;
    }
}