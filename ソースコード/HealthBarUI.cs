using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.3f, 0);
    public float animationDuration = 0.3f;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void SetHealth(float current, float max)
    {
        float targetFill = Mathf.Clamp01(current / max);
        fillImage.DOFillAmount(targetFill, animationDuration).SetEase(Ease.OutQuad);
    }
}
