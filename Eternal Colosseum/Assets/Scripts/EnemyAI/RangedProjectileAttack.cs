using UnityEngine;

public class RangedProjectileAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform firePoint;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileDamage = 10f;

    private void Awake()
    {
        if (brain == null)
            brain = GetComponent<EnemyBrain>();
    }

    private void OnEnable()
    {
        if (brain != null && brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnLoose += FireProjectile;
    }

    private void OnDisable()
    {
        if (brain != null && brain.EnemyAnimator != null)
            brain.EnemyAnimator.OnLoose -= FireProjectile;
    }

    private void FireProjectile()
    {
        if (projectilePool == null || brain == null || brain.Player == null) return;

        Vector3 startPosition = firePoint != null
            ? firePoint.position
            : transform.position + transform.forward + Vector3.up * 1.2f;

        Vector3 direction = (brain.Player.position + Vector3.up * 1f - startPosition).normalized;

        EnemyProjectile projectile = projectilePool.GetProjectile();
        projectile.Launch(startPosition, direction, projectileDamage, projectilePool);

        Debug.Log("[Ranged] Projectile fired.");
    }
}