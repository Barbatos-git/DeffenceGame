using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitButtonController : MonoBehaviour, IPointerClickHandler
{
    public int unitIndex;
    public float cooldownTime = 5f;
    public Image cooldownOverlay;

    private float cooldownTimer = 0f;
    private bool isCooling = false;

    private Button button;
    private UnitPlayer unitPlayer;

    void Start()
    {
        button = GetComponent<Button>();
        unitPlayer = FindObjectOfType<UnitPlayer>();
        cooldownOverlay.fillAmount = 0f;
    }

    void Update()
    {
        if (isCooling)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownOverlay.fillAmount = cooldownTimer / cooldownTime;

            if (cooldownTimer <= 0f)
            {
                isCooling = false;
                button.interactable = true;
                cooldownOverlay.fillAmount = 0f;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCooling || unitPlayer == null) return;

        unitPlayer.SelectUnit(unitIndex, this);
    }

    public void StartCooldown()
    {
        isCooling = true;
        cooldownTimer = cooldownTime;
        button.interactable = false;
        cooldownOverlay.fillAmount = 1f;
    }

    public bool IsCooling()
    {
        return isCooling;
    }
}
