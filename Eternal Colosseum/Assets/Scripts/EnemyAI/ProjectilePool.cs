using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int initialSize = 10;

    private readonly Queue<EnemyProjectile> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            EnemyProjectile projectile = CreateProjectile();
            pool.Enqueue(projectile);
        }
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile =
            Instantiate(projectilePrefab, transform);

        projectile.gameObject.SetActive(false);

        return projectile;
    }

    public EnemyProjectile GetProjectile()
    {
        if (pool.Count == 0)
        {
            pool.Enqueue(CreateProjectile());
        }

        return pool.Dequeue();
    }

    public void ReturnProjectile(EnemyProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }
}