using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;
using Features.Signing;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class TutorialBossController : MonoBehaviour
{
    [Header("Tutorial Boss Info")]
    public string BossName { get; set; } = "Tutorial Boss";

    [Header("Tutorial Boss Stats")]
    [SerializeField] [Range(1, 10)] private int maxHealth = 10;
    [SerializeField] [Range(1, 5)] private int maxDamage = 5;
    public int CurrentHealth { get; private set; }
    // In case bosses support power ups or scaled damage?
    public int CurrentDamage { get; private set; }

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
    public string impossibleGlyph = "Impossible";

    private SessionSelection _sessionSelection;
    private EnemyLabel _activeLabel;
    private bool _inImpossiblePhase = false;

    private void Awake()
    {
        InitHealth();
        CacheLabels();
    }

    // Temp work around to allow multiple labels on one enemy.
    public void InitSession(SessionSelection session)
    {
        _sessionSelection = session;
    }

    public void Damage(int amount = 1)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Max(1, amount));
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Toast.Instance.ShowToast($"Quinn dealt {amount} damage to {BossName}! HP: {CurrentHealth} / {maxHealth}", 1.5f, new Vector2(0f, 0f), new Vector2((Screen.width * 1.5f), 0f));
    }
    
    public void Attack(bool showToast = true)
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        health.Damage(Mathf.Max(1, Mathf.Min(CurrentDamage, maxDamage)), showToast);
    }

    public void HandleSignedWord(EnemyLabel label, int damage)
    {
        if (!_inImpossiblePhase) Damage(damage);
        else
        {
            Toast.Instance.ShowToast(
                $"{BossName} dodged Quinn's attack!", 
                1.5f, new Vector2(0f, 0f),
                new Vector2((Screen.width * 1.5f), 0f));
            return;
        }

        if (CurrentHealth <= 0)
        {
            // This should never occur but added just so that we can make sure nothing happens here.
            Debug.LogError("Tutorial Boss should never reach 0 hp");
            return;
        }

        if (CurrentHealth <= impasseThreshold * maxHealth)
        {
            EnterImpossiblePhase(label);
            return;
        }

        if (label && _sessionSelection && _sessionSelection.TryPop(out var next))
            label.SetWord(next);
        else
            EnterImpossiblePhase(label);
    }

    private void EnterImpossiblePhase(EnemyLabel label)
    {
        //Debug.Log("Triggering Impossible Phase");
        _inImpossiblePhase = true;
        _activeLabel = label;
        StartCoroutine(CycleImpossibleGlyphs());
    }

    private IEnumerator CycleImpossibleGlyphs()
    {
        while (_inImpossiblePhase && _activeLabel)
        {
            _activeLabel.SetWord(GenerateRandomGlyph());
            yield return new WaitForSeconds(0.75f);
            Attack(showToast: false);
        }
    }

    private string GenerateRandomGlyph()
    {
        string glyphPool = "0123456789)!#$%^&*(./;'[]<?\"\\{}";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < UnityEngine.Random.Range(2, 8); i++)
        {
            sb.Append(glyphPool[UnityEngine.Random.Range(0, glyphPool.Length)]);
        }
        return sb.ToString();
    }

    private void InitHealth()
    {
        CurrentHealth = Mathf.Clamp(maxHealth, 1, 10);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
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

