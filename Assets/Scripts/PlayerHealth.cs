using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private GUIStyle healthDisplayStyle;
    private PlayerHitOverlay hitOverlay;
    private Material deadMaterial;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        if (GetComponent<PlayerControlsHelp>() == null)
            gameObject.AddComponent<PlayerControlsHelp>();
        hitOverlay = GetComponent<PlayerHitOverlay>();
        if (hitOverlay == null)
            hitOverlay = gameObject.AddComponent<PlayerHitOverlay>();
        Debug.Log($"플레이어 체력 초기화: {currentHealth}/{maxHealth}", this);
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

#if UNITY_STANDALONE && !UNITY_EDITOR
        Application.Quit();
#else
        var menu = GetComponent<PlayerDeathMenu>();
        if (menu == null) menu = gameObject.AddComponent<PlayerDeathMenu>();
        menu.Show();
        menu.EndGame();
#endif
    }

    void OnGUI()
    {
        if (healthDisplayStyle == null)
        {
            healthDisplayStyle = new GUIStyle(GUI.skin.label);
            healthDisplayStyle.fontSize = 22;
            healthDisplayStyle.fontStyle = FontStyle.Bold;
            healthDisplayStyle.alignment = TextAnchor.MiddleCenter;
            healthDisplayStyle.normal.textColor = Color.white;
        }

        // Keep the HUD inside the safe area, including on narrow screens.
        Rect safeArea = Screen.safeArea;
        float width = Mathf.Min(260f, safeArea.width - 24f);
        if (width <= 0f)
            return;
        Rect panel = new Rect(safeArea.xMax - width - 12f,
            Screen.height - safeArea.yMax + 20f, width, 64f);
        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;
        GUI.depth = -100;
        GUI.color = new Color(0.06f, 0.07f, 0.1f, 0.92f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        Rect bar = new Rect(panel.x + 10f, panel.y + 42f, panel.width - 20f, 12f);
        GUI.color = new Color(0.22f, 0.08f, 0.1f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        float fraction = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        GUI.color = new Color(0.9f, 0.18f, 0.25f);
        GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * fraction, bar.height),
            Texture2D.whiteTexture);
        GUI.color = Color.white;
        string label = IsDead
            ? $"HP: 0 / {maxHealth} (DEAD)"
            : $"HP: {currentHealth} / {maxHealth}";
        GUI.Label(new Rect(panel.x, panel.y + 4f, panel.width, 34f),
            label, healthDisplayStyle);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        int restored = Mathf.Min(amount, maxHealth - currentHealth);
        currentHealth += restored;
        Debug.Log($"플레이어 체력 회복: {restored}, 현재 체력: {currentHealth}/{maxHealth}", this);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        hitOverlay.Play();
        Debug.Log($"플레이어 피격: {damage}, 남은 체력: {currentHealth}/{maxHealth}", this);

        if (IsDead)
        {
            Die();
            Debug.Log("플레이어 체력이 0이 되었습니다.", this);
        }
    }

    private void Die()
    {
        var attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.enabled = false;
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        if (hitOverlay != null) hitOverlay.enabled = false;

        var animator = GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetFloat("Speed", 0f);
            animator.Play("Base Layer.WGS_Stand@loop", 0, 0f);
            animator.Update(0f);
            animator.speed = 0f;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        deadMaterial = new Material(shader) { color = new Color(0.45f, 0.45f, 0.45f, 1f) };
        foreach (Renderer model in GetComponentsInChildren<Renderer>(true))
        {
            if (!model.enabled || !(model is MeshRenderer || model is SkinnedMeshRenderer))
                continue;
            var materials = model.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = deadMaterial;
            model.sharedMaterials = materials;
        }

        var deathMenu = GetComponent<PlayerDeathMenu>();
        if (deathMenu == null) deathMenu = gameObject.AddComponent<PlayerDeathMenu>();
        deathMenu.Show();
    }

    void OnDestroy()
    {
        if (deadMaterial != null) Destroy(deadMaterial);
    }
}
