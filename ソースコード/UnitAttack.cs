using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;

public class UnitAttack : MonoBehaviour
{
    [Header("ユニット設定")]
    public string targetTag = "Enemy"; // 例：士兵は "Enemy"，敵は "PlayerUnit"
    public bool canAttackTower = false;
    public Transform fallbackTarget; // 城（塔）の Transform

    [Header("攻撃設定")]
    public float attackRange = 2f;
    public float attackInterval = 1.5f;
    public int damage = 10;
    public float searchRadius = 8f;
    public LayerMask detectionLayer = ~0;

    private float attackTimer = 0f;
    private Transform currentTarget;
    private NavMeshAgent agent;
    private Animator anim;

    private float searchInterval = 0.3f;
    private float lastSearchTime = 0f;

    [Header("パトロール設定")]
    public bool enablePatrol = false; // パトロールを有効にするか
    public float patrolRadius = 5f; // パトロール半径
    private Vector3 patrolDestination;
    private bool isPatrolling = false;

    private float smoothSpeed = 0f;
    private float smoothTime = 0.1f;

    [Header("攻撃効果音")]
    public AudioClip attackClip;
    private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (enablePatrol)
        {
            SetNewPatrolDestination();
        }
    }

    void Update()
    {
        float rawSpeed = agent.velocity.magnitude;
        smoothSpeed = Mathf.Lerp(smoothSpeed, rawSpeed, Time.deltaTime / smoothTime);
        if (smoothSpeed < 0.05f)
            smoothSpeed = 0f;

        anim.SetFloat("Speed", smoothSpeed);

        // 死亡チェック（自分が死んでいれば処理中止）
        var selfHealth = GetComponent<IDamageable>();
        if (selfHealth != null && selfHealth.IsDead()) return;

        // ターゲットがいない・死亡 → 再探索
        var damageable = currentTarget != null ? currentTarget.GetComponent<IDamageable>() : null;
        if (currentTarget == null || damageable == null || damageable.IsDead() || currentTarget == fallbackTarget)
        {
            if (Time.time - lastSearchTime > searchInterval)
            {
                currentTarget = FindClosestTarget();
                lastSearchTime = Time.time;
                if (currentTarget == null)
                {
                    // 敵がいなければ巡回開始
                    if (!isPatrolling && enablePatrol)
                    {
                        SetNewPatrolDestination();
                        isPatrolling = true;
                    }
                }
                else
                {
                    // 敵を見つけたら巡回終了
                    isPatrolling = false;
                }
            }
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist <= attackRange)
            {
                agent.isStopped = true;
                transform.LookAt(currentTarget);
                attackTimer += Time.deltaTime;

                if (attackTimer >= attackInterval)
                {
                    attackTimer = 0f;
                    anim.ResetTrigger("Attack");
                    anim.SetTrigger("Attack");
                    audioSource.PlayOneShot(attackClip);
                    StartCoroutine(DelayedDamage(0.6f));
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
        }
        else if (isPatrolling && enablePatrol)
        {
            // 巡回中、目的地にほぼ到達したら新しい巡回先をセット
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetNewPatrolDestination();
            }
        }
    }

    IEnumerator DelayedDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        DoDamage();
    }

    // 攻撃アニメーションのイベントから呼び出す
    public void DoDamage()
    {
        if (currentTarget == null) return;

        var damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead())
        {
            damageable.TakeDamage(damage);
        }
    }

    Transform FindClosestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, detectionLayer);
        Debug.Log($"{name} 寻找目标：命中数量 {hits.Length}");

        GameObject nearest = hits
           .Where(c => c.CompareTag(targetTag))
           .Select(c => c.gameObject)
           .OrderBy(go => Vector3.Distance(transform.position, go.transform.position))
           .FirstOrDefault();

        if (nearest != null)
        {
            Debug.Log($"{name} 找到了目标：{nearest.name}");
            return nearest.transform;
        }

        // ターゲットが見つからない場合 → 城を攻撃
        if (canAttackTower && fallbackTarget != null)
        {
            var tower = fallbackTarget.GetComponent<IDamageable>();
            if (tower != null && !tower.IsDead())
            {
                return fallbackTarget;
            }
        }

        return null;
    }

    // 敵が見つからない時に呼ばれるパトロール処理
    void SetNewPatrolDestination()
    {
        Vector3 patrolCenter = transform.position;
        Vector3 destination;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(2f, patrolRadius);
            Vector3 randomPoint = new Vector3(randomCircle.x, 0, randomCircle.y);
            destination = patrolCenter + randomPoint;

            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, hit.position);
                if (dist >= 2f)
                {
                    patrolDestination = hit.position;
                    agent.SetDestination(patrolDestination);
                    agent.isStopped = false;
                    isPatrolling = true;
                    return;
                }
            }
        }

        Debug.LogWarning($"{name} 巡逻目标设置失败，停止移动");
        agent.isStopped = true;
        isPatrolling = false;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
