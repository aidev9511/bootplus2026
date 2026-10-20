using UnityEngine;

[RequireComponent(typeof(SlimeHealth))]
public class SlimeAI : MonoBehaviour
{
    [Header("플레이어")]
    public Transform player;

    [Header("이동 설정")]
    public float moveSpeed = 1f;
    public float detectionRange = 4f;
    public float stopDistance = 1.2f;

    [Header("공격 설정 (Stop Distance 안에서 공격)")]
    [Min(1)] public int attackDamage = 10;
    [Min(0.01f)] public float attackInterval = 2f;

    [Header("랜덤 배회")]
    public float wanderRadius = 3f;
    public float wanderInterval = 2f;

    [Header("바운스 애니메이션")]
    public Transform visual;
    public float bounceHeight = 0.18f;
    public float bounceSpeed = 7f;
    public float squashAmount = 0.12f;

    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private float nextWanderTime;

    private Vector3 visualStartPosition;
    private Vector3 visualStartScale;
    private SlimeHealth health;
    private float bouncePhase;
    private bool isMoving;
    private float nextAttackTime;
    private bool warnedMissingPlayerHealth;

    void Start()
    {
        health = GetComponent<SlimeHealth>();

        if (player == null)
        {
            GameObject target = GameObject.Find("Player");

            if (target != null)
                player = target.transform;
        }

        spawnPosition = transform.position;

        
        // Visual이 비어 있으면 자식 모델 자동 검색
        if (visual == null)
        {
            visual = transform.Find("Slime");
        }

        if (visual != null && visual != transform)
        {
            visualStartPosition = visual.localPosition;
            visualStartScale = visual.localScale;

            Debug.Log(
                gameObject.name +
                " 바운스 모델 연결 성공: " +
                visual.name
            );
        }
        else
        {
            Debug.LogWarning(
                gameObject.name +
                " 바운스 모델을 찾지 못했습니다."
            );
        }

        ChooseWanderTarget();
    }

    void Update()
    {
        if (health != null && health.CurrentHealth <= 0)
            return;

        isMoving = false;

        if (player != null)
        {
            Vector3 toPlayer =
                player.position - transform.position;

            toPlayer.y = 0f;

            float distance = toPlayer.magnitude;

            if (distance <= detectionRange)
            {
                if (distance > stopDistance)
                    MoveTowards(player.position);
                else if (Time.timeScale > 0f && Time.time >= nextAttackTime)
                    AttackPlayer();
            }
            else
            {
                Wander();
            }
        }

        UpdateBounce();
    }

    void AttackPlayer()
    {
        PlayerHealth playerHealth = player.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            if (!warnedMissingPlayerHealth)
            {
                Debug.LogWarning("슬라임 공격 대상에 PlayerHealth가 없습니다. " +
                    "Player 오브젝트에 PlayerHealth를 추가하세요.", player);
                warnedMissingPlayerHealth = true;
            }
            return;
        }

        warnedMissingPlayerHealth = false;
        if (playerHealth.IsDead)
            return;

        playerHealth.TakeDamage(attackDamage);
        nextAttackTime = Time.time + Mathf.Max(0.01f, attackInterval);
    }

    void Wander()
    {
        Vector3 toTarget = wanderTarget - transform.position;
        toTarget.y = 0f;

        if (Time.time >= nextWanderTime ||
            toTarget.magnitude < 0.3f)
        {
            ChooseWanderTarget();
        }

        MoveTowards(wanderTarget);
    }

    void ChooseWanderTarget()
    {
        Vector2 random =
            Random.insideUnitCircle * wanderRadius;

        wanderTarget = spawnPosition +
            new Vector3(random.x, 0f, random.y);

        nextWanderTime =
            Time.time + wanderInterval;
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Vector3 movement =
            direction.normalized * moveSpeed * Time.deltaTime;

        movement = Vector3.ClampMagnitude(
            movement,
            direction.magnitude
        );

        transform.position += movement;
        isMoving = true;
    }

    void UpdateBounce()
    {
        if (visual == null || visual == transform)
            return;
/*
        if (!isMoving)
        {
            // 멈추면 기본 모습으로 돌아가기
            visual.localPosition = Vector3.Lerp(
                visual.localPosition,
                visualStartPosition,
                Time.deltaTime * 10f
            );

            visual.localScale = Vector3.Lerp(
                visual.localScale,
                visualStartScale,
                Time.deltaTime * 10f
            );

            return;
        }*/

        bouncePhase += Time.deltaTime * bounceSpeed;

        float wave = Mathf.Abs(Mathf.Sin(bouncePhase));

        // 몸통이 위아래로 움직임
        visual.localPosition =
            visualStartPosition +
            Vector3.up * wave * bounceHeight;

        // 착지 시 약간 납작해지는 효과
        float squash = 1f - (1f - wave) * squashAmount;

        visual.localScale = new Vector3(
            visualStartScale.x / Mathf.Sqrt(squash),
            visualStartScale.y * squash,
            visualStartScale.z / Mathf.Sqrt(squash)
        );
    }

    // 레벨별 난이도 설정
    public void SetDifficulty(
        float speed,
        float range,
        float radius)
    {
        moveSpeed = speed;
        detectionRange = range;
        wanderRadius = radius;
    }
}
