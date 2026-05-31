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
    
    // animator parameters
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LightAttackHash = Animator.StringToHash("LightAttack");
    private static readonly int HeavyWindupHash = Animator.StringToHash("HeavyWindup");
    private static readonly int HeavyAttackHash = Animator.StringToHash("HeavyAttack");
    private static readonly int DashWindupHash = Animator.StringToHash("DashWindup");
    private static readonly int DashAttackHash = Animator.StringToHash("DashAttack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private NavMeshAgent agent;
    private Animator animator;

    private bool isAttacking;
    private float nextAttackTime;
    private bool isDead;

    private float _pendingDamage;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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
        if (player == null || isAttacking || isDead) return;

        FacePlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);

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
                animator.SetTrigger(LightAttackHash);
                _pendingDamage = lightAttackDamage;
                Debug.Log("[Boss] Light Attack");
                yield return new WaitForSeconds(0.35f);
                break;

            case BossAttackType.HeavyAttack:
                // Step 1 — play telegraph, hold on last frame
                animator.SetTrigger(HeavyWindupHash);
                yield return new WaitForSeconds(heavyTelegraphTime);
                // Step 2 — release into the heavy swing
                _pendingDamage = heavyAttackDamage;
                animator.SetTrigger(HeavyAttackHash);
                yield return new WaitForSeconds(0.5f);
                // AnimEvent fires at impact frame of HeavyAttack clip
                break;

            case BossAttackType.DashAttack:
                // Step 1 — play telegraph, hold on last frame
                animator.SetTrigger(DashWindupHash);
                yield return new WaitForSeconds(dashTelegraphTime);
                // Step 2 — dash and release into the lunge
                DashTowardPlayer();
                _pendingDamage = dashAttackDamage;
                animator.SetTrigger(DashAttackHash);
                yield return new WaitForSeconds(0.4f);
                // AnimEvent fires at impact frame of DashAttack clip
                break;

        }

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    public void AnimEvent_DealDamage(float damage)
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
            playerHealth.TakeDamage(_pendingDamage);
            Debug.Log($"[Boss] Player hit for {_pendingDamage} damage.");
        }

        _pendingDamage = 0f;
    }

    //Public hit/death entry points
    //when the boss takes a hit
    public void OnHit()
    {
        if (isDead) return;
        ResetAttackTriggers();
        animator.SetTrigger(HitHash);
    }

    //when the boss dies
    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = true; // prevents Update from starting new attacks
        StopMoving();
        ResetAttackTriggers();
        animator.SetTrigger(DeathHash);
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

    private void ResetAttackTriggers()
    {
        animator.ResetTrigger(LightAttackHash);
        animator.ResetTrigger(HeavyWindupHash);
        animator.ResetTrigger(HeavyAttackHash);
        animator.ResetTrigger(DashWindupHash);
        animator.ResetTrigger(DashAttackHash);
    }

}