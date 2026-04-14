using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;
using System;
using UnityEngine;

public class TutorialBossController : MonoBehaviour
{
    [Header("Tutorial Boss Info")]
    public string bossName { get; set; }

    [Header("Tutorial Boss Stats")]
    [SerializeField] [Range(1, 10)] private int maxHealth = 10;
    [SerializeField] [Range(1, 5)] private int maxDamage = 5;
    public int currentHealth { get; private set; }
    // In case bosses support power ups or scaled damage?
    public int currentDamage { get; private set; }

    // Mostly same system as enemies other than multiple labels being possible
    [Header("Death FX")]
    [SerializeField] private ParticleSystem explodeVfx;
    [SerializeField] private AudioClip explodeSfx;
    [SerializeField] private float despawnDelay = 2f;
    private bool _isDead;
    private EnemyLabel[] _labelsCache;

    public event Action<int, int> OnHealthChanged;

    [Header("Impossible Glyph (Sciprted Loss)")]
    [SerializeField] private float impasseThreshold = 0.25f;
    public string impossibleGlyph;
    public event Action OnImpossiblePhaseChange;

    private void Awake()
    {
        InitHealth();
        CacheLabels();
    }

    public void Damage(int amount = 1)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, amount));
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Toast.Instance.ShowToast($"Quinn dealt {amount} damage to {bossName}!", 1.5f, new Vector2(0f, 0f), new Vector2((Screen.width * 1.5f), 0f));
        if (currentHealth <= impasseThreshold * maxHealth) // Simulate impossible sign
        {
            OnImpossiblePhaseChange?.Invoke();
        }
    }
    
    public void Attack()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        health.Damage(Mathf.Max(1, Mathf.Min(currentDamage, maxDamage)));
    }

    public void HandleSignedWord(EnemyLabel label, int damage)
    {
        if (label) Destroy(label.gameObject);
        Damage(damage);
    }

    private void InitHealth()
    {
        currentHealth = Mathf.Clamp(maxHealth, 1, 10);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void CacheLabels()
    {
        _labelsCache = GetComponentsInChildren<EnemyLabel>(true);
    }


    // Explode calls the series of methods below it.
    public void Explode()
    {
        if (_isDead) return;
        _isDead = true;

        StopBehaviors();
        StopPhysics();
        HideRenderer();
        DestroyLabels();
        TriggerDeathFX();
        Destroy(gameObject, despawnDelay);
    }

    private void StopBehaviors()
    {
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == this) continue;
            mb.enabled = false;
        }
    }

    private void StopPhysics()
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.Sleep();
        }
        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.enabled = false;
    }

    private void HideRenderer()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = false;
    }

    private void DestroyLabels()
    {
        foreach (EnemyLabel l in _labelsCache)
        {   
            if (l) Destroy(l.gameObject);
        }
    }

    private void TriggerDeathFX()
    {
        if (explodeVfx)
        {
            var v = Instantiate(explodeVfx, transform.position, transform.rotation);
            v.Play();
            Destroy(v.gameObject, v.main.duration + v.main.startLifetime.constantMax + 0.25f);
        }
        if (explodeSfx) AudioSource.PlayClipAtPoint(explodeSfx, transform.position);
    }
}

