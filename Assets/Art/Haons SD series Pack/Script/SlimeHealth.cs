using System.Collections;
using UnityEngine;

public class SlimeHealth : MonoBehaviour
{
    [Header("슬라임 체력")]
    [SerializeField] private int maxHealth = 30;

    [Header("보너스 HP 물약")]
    [SerializeField, Range(0f, 1f)] private float potionDropChance = 0.25f;
    [SerializeField, Min(1)] private int potionHealAmount = 50;

    private int currentHealth;
    private bool isDead = false;

    // PlayerAttack에서 현재 체력 확인
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private GUIStyle healthLabelStyle;

    // Screen-space labels follow the model bounds, including the larger boss.
    void OnGUI()
    {
        if (isDead || renderers == null || Time.timeScale <= 0f)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        foreach (Renderer model in renderers)
        {
            if (model == null || !model.enabled || !model.gameObject.activeInHierarchy ||
                !(model is MeshRenderer || model is SkinnedMeshRenderer))
                continue;
            if (!hasBounds)
                bounds = model.bounds;
            else
                bounds.Encapsulate(model.bounds);
            hasBounds = true;
        }

        Vector3 head = hasBounds
            ? new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z)
            : transform.position + Vector3.up * 1.5f;
        Vector3 screen = camera.WorldToScreenPoint(head);
        if (screen.z <= 0f || !camera.pixelRect.Contains(new Vector2(screen.x, screen.y)))
            return;

        if (healthLabelStyle == null)
        {
            healthLabelStyle = new GUIStyle(GUI.skin.label);
            healthLabelStyle.fontSize = 14;
            healthLabelStyle.fontStyle = FontStyle.Bold;
            healthLabelStyle.alignment = TextAnchor.MiddleCenter;
            healthLabelStyle.normal.textColor = Color.white;
        }

        Rect panel = new Rect(screen.x - 65f, Screen.height - screen.y - 34f, 130f, 34f);
        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;
        GUI.depth = -50;
        GUI.color = new Color(0.05f, 0.06f, 0.08f, 0.45f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        Rect bar = new Rect(panel.x + 5f, panel.y + 24f, panel.width - 10f, 6f);
        GUI.color = new Color(0.25f, 0.07f, 0.08f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.2f, 0.25f);
        float ratio = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * ratio, bar.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(panel.x, panel.y, panel.width, 23f),
            $"HP {currentHealth} / {maxHealth}", healthLabelStyle);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    // ★ 추가: SlimeSpawner에서 레벨별 체력을 설정
    public void SetMaxHealth(int health)
    {
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;

        Debug.Log(
            gameObject.name +
            " 최대 체력 설정: " + maxHealth
        );
    }

    [Header("피격 효과")]
    public Color hitColor = Color.red;
    public float hitDuration = 0.15f;
    public float hitScale = 0.85f;

    [Header("사망 효과")]
    public GameObject deathEffectPrefab;
    public float deathDuration = 0.4f;
    public float deathEffectHeight = 0.3f;

    [Header("퀘스트 설정")]
    public QuestManager questManager;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private Color[][] originalColors;
    private Vector3 originalScale;
    private Coroutine hitCoroutine;

    void Start()
    {
        // SlimeSpawner에서 설정한 최대 체력 사용
        currentHealth = maxHealth;
        originalScale = transform.localScale;

        // QuestManager 자동 검색
        if (questManager == null)
        {
            questManager =
                FindFirstObjectByType<QuestManager>();
        }

        // 슬라임 Renderer 검색
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        originalColors = new Color[renderers.Length][];

        // 슬라임 원래 색상 저장
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials =
                renderers[i].sharedMaterials;

            originalColors[i] =
                new Color[materials.Length];

            for (int j = 0; j < materials.Length; j++)
            {
                Material mat = materials[j];

                if (mat != null &&
                    mat.HasProperty("_BaseColor"))
                {
                    originalColors[i][j] =
                        mat.GetColor("_BaseColor");
                }
                else
                {
                    originalColors[i][j] = Color.white;
                }
            }
        }

        Debug.Log(
            gameObject.name + " 체력: " + currentHealth
        );
    }

    // 슬라임 데미지 처리
    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth =
            Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            gameObject.name +
            " 남은 체력: " + currentHealth
        );

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 기존 피격 효과 중지
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            RestoreAppearance();
        }

        // 빨간색 피격 효과 실행
        hitCoroutine =
            StartCoroutine(PlayHitEffect());
    }

    // 일반 피격 효과
    IEnumerator PlayHitEffect()
    {
        transform.localScale =
            originalScale * hitScale;

        SetHitColor(hitColor);

        yield return new WaitForSeconds(hitDuration);

        RestoreAppearance();
        hitCoroutine = null;
    }

    // 슬라임 사망 시작
    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 사망 확정 시 한 번만 회복하며, 최대 체력은 넘지 않는다.
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Heal(30);

        Debug.Log(
            gameObject.name + " 처치 완료!"
        );

        // 퀘스트 처치 카운트 증가
        if (questManager == null)
        {
            questManager =
                FindFirstObjectByType<QuestManager>();
        }

        if (questManager != null)
        {
            questManager.AddKill(this);

            Debug.Log("퀘스트 처치 카운트 +1");
        }
        else
        {
            Debug.LogWarning(
                "QuestManager가 연결되지 않았습니다."
            );
        }

        // 기존 피격 효과 중지
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        // 사망 애니메이션 실행
        StartCoroutine(PlayDeathEffect());

        // Optional loot must never interrupt the kill count or death animation.
        if (Random.value < potionDropChance)
        {
            try
            {
                HealthPotionPickup.Spawn(transform.position, potionHealAmount);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    // 사망 애니메이션
    IEnumerator PlayDeathEffect()
    {
        SetHitColor(hitColor);

        // 사망 중 충돌 비활성화
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        // 슬라임이 점점 작아지는 연출
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(
                    0.01f,
                    deathDuration
                )
            );

            transform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                t
            );

            yield return null;
        }

        // 사망 파티클 생성
        if (deathEffectPrefab != null)
        {
            Vector3 effectPosition =
                transform.position +
                Vector3.up * deathEffectHeight;

            Instantiate(
                deathEffectPrefab,
                effectPosition,
                Quaternion.identity
            );
        }

        // 슬라임 제거
        Destroy(gameObject);
    }

    // 슬라임 색상 변경
    void SetHitColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            Material[] materials =
                renderer.sharedMaterials;

            for (int j = 0; j < materials.Length; j++)
            {
                Material mat = materials[j];

                if (mat == null ||
                    !mat.HasProperty("_BaseColor"))
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock, j
                );

                propertyBlock.SetColor(
                    "_BaseColor",
                    color
                );

                renderer.SetPropertyBlock(
                    propertyBlock, j
                );
            }
        }
    }

    // 슬라임 원래 색상 및 크기로 복구
    void RestoreAppearance()
    {
        transform.localScale = originalScale;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            Material[] materials =
                renderer.sharedMaterials;

            for (int j = 0; j < materials.Length; j++)
            {
                Material mat = materials[j];

                if (mat == null ||
                    !mat.HasProperty("_BaseColor"))
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock, j
                );

                propertyBlock.SetColor(
                    "_BaseColor",
                    originalColors[i][j]
                );

                renderer.SetPropertyBlock(
                    propertyBlock, j
                );
            }
        }
    }
}
