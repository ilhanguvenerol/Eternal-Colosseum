using UnityEngine;
using UnityEngine.AI;

public class BossBrain : MonoBehaviour
{
    private enum BossAttackType
    {
        LightAttack,
        HeavyAttack,
        DashAttack
    }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 2.8f;

    [Header("Combat")]
    [SerializeField] private float lightAttackDamage = 12f;
    [SerializeField] private float heavyAttackDamage = 25f;
    [SerializeField] private float dashAttackDamage = 18f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 3f;

    [Header("Telegraph")]
    [SerializeField] private float heavyTelegraphTime = 0.8f;
    [SerializeField] private float dashTelegraphTime = 0.5f;

    private NavMeshAgent agent;
    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = false;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || isAttacking) return;

        FacePlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            MoveToPlayer();
            return;
        }

        StopMoving();

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine(PickAttack()));
        }
    }

    private BossAttackType PickAttack()
    {
        float roll = Random.value;

        if (roll < 0.55f)
            return BossAttackType.LightAttack;

        if (roll < 0.8f)
            return BossAttackType.HeavyAttack;

        return BossAttackType.DashAttack;
    }

    private System.Collections.IEnumerator AttackRoutine(BossAttackType attackType)
    {
        isAttacking = true;
        StopMoving();

        switch (attackType)
        {
            case BossAttackType.LightAttack:
                Debug.Log("[Boss] Light Attack");
                yield return new WaitForSeconds(0.35f);
                DealDamage(lightAttackDamage);
                break;

            case BossAttackType.HeavyAttack:
                Debug.Log("[Boss] Heavy Attack Telegraph");
                yield return new WaitForSeconds(heavyTelegraphTime);
                Debug.Log("[Boss] Heavy Attack Hit");
                DealDamage(heavyAttackDamage);
                break;

            case BossAttackType.DashAttack:
                Debug.Log("[Boss] Dash Attack Telegraph");
                yield return new WaitForSeconds(dashTelegraphTime);
                DashTowardPlayer();
                yield return new WaitForSeconds(0.25f);
                DealDamage(dashAttackDamage);
                break;
        }

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private void DealDamage(float damage)
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange + 0.75f) return;

        PlayerCombatState playerCombat = player.GetComponent<PlayerCombatState>();

        if (playerCombat != null && playerCombat.IsParrying)
        {
            Debug.Log("[Boss] Attack parried!");
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"[Boss] Player hit for {damage} damage.");
        }
    }

    private void MoveToPlayer()
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void StopMoving()
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void DashTowardPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        transform.position += direction * 2f;
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}