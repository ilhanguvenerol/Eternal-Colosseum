using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float damage = 10f;

    private Vector3 direction;
    private float timer;
    private ProjectilePool ownerPool;
    private bool hasHit;

    public void Launch(Vector3 startPosition, Vector3 shootDirection, float projectileDamage, ProjectilePool pool)
    {
        transform.position = startPosition;
        direction = shootDirection.normalized;
        damage = projectileDamage;
        ownerPool = pool;
        timer = lifeTime;
        hasHit = false;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[Projectile] Player hit for {damage} damage.");
            }

            ReturnToPool();
        }
        else if (!other.isTrigger)
        {
            hasHit = true;
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnProjectile(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}