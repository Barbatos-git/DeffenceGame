using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class UnitHealth : MonoBehaviour, IDamageable
{
    public int maxHP = 100;
    private int currentHP;
    private Animator anim;
    private bool isDead = false;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private HealthBarUI healthBarInstance;

    void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();

        if (healthBarPrefab != null)
        {
            GameObject ui = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity);
            ui.transform.SetParent(transform);
            healthBarInstance = ui.GetComponent<HealthBarUI>();
            healthBarInstance.target = transform;
            healthBarInstance.SetHealth(currentHP, maxHP);
        }
    }

    void Update()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHP, maxHP);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHP, maxHP);
        }


        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        if (CompareTag("Enemy"))
        {
            GameStats.killCount++;
        }

        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
        }

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        foreach (var c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }

        StartCoroutine(WaitAndSink(1.5f));
    }

    IEnumerator WaitAndSink(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(SinkAndDestroy());
    }

    IEnumerator SinkAndDestroy()
    {
        float sinkDuration = 2f;
        float sinkSpeed = 0.5f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < sinkDuration)
        {
            transform.position = startPos + Vector3.down * (elapsed * sinkSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return currentHP;
    }

    public float GetMaxHealth()
    {
        return maxHP;
    }

    public bool IsDead() => isDead;
}
