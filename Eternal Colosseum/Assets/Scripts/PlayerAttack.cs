using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask enemyLayer;

    // ─────────────────────────────────────────
    //  References
    // ─────────────────────────────────────────
    private PlayerMana playerMana;

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────
    private void Awake()
    {
        playerMana = GetComponent<PlayerMana>();
    }

    private void Update()
    {
        // Sol tık veya K tuşuyla saldır
        if (Keyboard.current.kKey.wasPressedThisFrame)
            Attack();
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────
    private void Attack()
    {
        Debug.Log("[Attack] Saldırı yapıldı!");

        // Oyuncunun önünde attackRange kadar sphere cast
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            attackRange, 
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(attackDamage, playerMana);
                Debug.Log($"[Attack] {hit.name} vuruldu!");
            }
        }
    }

    // Gizmos ile saldırı menzilini sahnede göster
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}