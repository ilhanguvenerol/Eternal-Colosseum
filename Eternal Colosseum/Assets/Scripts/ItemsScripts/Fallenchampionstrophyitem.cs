using UnityEngine;

[CreateAssetMenu(fileName = "FallenChampionsTrophy", menuName = "Inventory/Coliseum Items/Fallen Champion's Trophy")]
public class FallenChampionsTrophyItem : ItemData
{
    [Header("Orb Settings")]
    public float healAmount = 8f;       // Set this on the orb pickup script, not here
    public GameObject healOrbPrefab;    // Assign your healing orb prefab in the Inspector

    public override void OnKillEffect(GameObject player, GameObject killedEnemy)
    {
        if (healOrbPrefab == null) return;

        Instantiate(healOrbPrefab, killedEnemy.transform.position, Quaternion.identity);
        Debug.Log("[ITEM] Fallen Champion's Trophy — a healing orb left in their wake.");
    }
}