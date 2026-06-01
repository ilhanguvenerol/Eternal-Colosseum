using UnityEngine;

/// <summary>
/// Central audio system for Eternal Coliseum.
/// Attach to an empty GameObject named "AudioManager" in your scene.
/// Drag audio clips into the Inspector slots.
/// Other scripts call AudioManager.Instance.Play___() methods.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    //  Audio Sources 
    [Header("Audio Sources")]
    [Tooltip("Plays all combat and gameplay sound effects")]
    public AudioSource sfxSource;

    [Tooltip("Plays background music — looping, separate from SFX")]
    public AudioSource musicSource;

    //  Player Sounds 
    [Header("Player Sounds")]
    [Tooltip("Player swings their sword")]
    public AudioClip playerAttack;

    [Tooltip("Player successfully parries an attack")]
    public AudioClip playerParry;

    [Tooltip("Player takes damage")]
    public AudioClip playerHurt;

    [Tooltip("Player dies")]
    public AudioClip playerDeath;

    [Tooltip("Player heals (potion, item effect)")]
    public AudioClip playerHeal;

    // Enemy Sounds
    [Header("Enemy Sounds")]
    [Tooltip("Standard enemy attacks the player")]
    public AudioClip enemyAttack;

    [Tooltip("Standard enemy takes damage")]
    public AudioClip enemyHurt;

    [Tooltip("Standard enemy dies")]
    public AudioClip enemyDeath;

    // Boss Sounds 
    [Header("Boss Sounds")]
    [Tooltip("Boss attacks the player")]
    public AudioClip bossAttack;

    [Tooltip("Boss takes damage")]
    public AudioClip bossHurt;

    [Tooltip("Boss dies")]
    public AudioClip bossDeath;

    // Background Music
    [Header("Background Music")]
    [Tooltip("Music that plays in the main arena during combat")]
    public AudioClip arenaMusic;

    [Tooltip("Music that plays in the shop")]
    public AudioClip shopMusic;

    [Tooltip("Music that plays on the main menu")]
    public AudioClip mainMenuMusic;

    //  Item Sounds
    [Header("Item Sounds")]
    [Tooltip("Player buys or picks up an item")]
    public AudioClip itemPickup;

    [Tooltip("Played when an item effect triggers (proc)")]
    public AudioClip itemProc;

    // ─────────────────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Player Sound Methods
    //  Called from: PlayerCombatState.cs, PlayerHealth.cs
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Call in PlayerCombatState.TrySwordAttack() when attack fires</summary>
    public void PlayPlayerAttack() => PlaySFX(playerAttack);

    /// <summary>Call in PlayerCombatState.TryParry() when parry fires</summary>
    public void PlayPlayerParry() => PlaySFX(playerParry);

    /// <summary>Call in PlayerHealth.TakeDamage() after damage is applied</summary>
    public void PlayPlayerHurt() => PlaySFX(playerHurt);

    /// <summary>Call in PlayerHealth.Die()</summary>
    public void PlayPlayerDeath() => PlaySFX(playerDeath);

    /// <summary>Call in PlayerHealth.Heal() when healing happens</summary>
    public void PlayPlayerHeal() => PlaySFX(playerHeal);

    // ─────────────────────────────────────────────────────────────────────────
    //  Enemy Sound Methods
    //  Called from: EnemyHealth.cs, EnemyBrain.cs
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Call in EnemyBrain.BeginAttackApproach()</summary>
    public void PlayEnemyAttack() => PlaySFX(enemyAttack);

    /// <summary>Call in EnemyHealth.TakeDamage()</summary>
    public void PlayEnemyHurt() => PlaySFX(enemyHurt);

    /// <summary>Call in EnemyHealth.Die()</summary>
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);

    // ─────────────────────────────────────────────────────────────────────────
    //  Boss Sound Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Call in boss attack logic when boss attacks</summary>
    public void PlayBossAttack() => PlaySFX(bossAttack);

    /// <summary>Call in boss health script when boss takes damage</summary>
    public void PlayBossHurt() => PlaySFX(bossHurt);

    /// <summary>Call in boss health script when boss dies</summary>
    public void PlayBossDeath() => PlaySFX(bossDeath);

    // ─────────────────────────────────────────────────────────────────────────
    //  Item Sound Methods
    //  Called from: Inventory.cs, ShopManager.cs
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Call in Inventory.BuyItem() and Inventory.AddItem()</summary>
    public void PlayItemPickup() => PlaySFX(itemPickup);

    /// <summary>Call in item OnHitEffect / OnKillEffect when a proc fires</summary>
    public void PlayItemProc() => PlaySFX(itemProc);

    // ─────────────────────────────────────────────────────────────────────────
    //  Music Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Call when the arena/combat scene loads</summary>
    public void PlayArenaMusic() => PlayMusic(arenaMusic);

    /// <summary>Call when the shop opens (InventoryToggle or ShopManager)</summary>
    public void PlayShopMusic() => PlayMusic(shopMusic);

    /// <summary>Call when the main menu loads</summary>
    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);

    /// <summary>Stop whatever music is playing</summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Core Logic
    // ─────────────────────────────────────────────────────────────────────────

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return; // Silently skip audio calls if no clip assigned (prevents errors for unassigned clips)
        if (sfxSource == null)
        {
            Debug.LogWarning("[AudioManager] SFX Source is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource == null)
        {
            Debug.LogWarning("[AudioManager] Music Source is not assigned!");
            return;
        }

        // Don't restart if same track is already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}