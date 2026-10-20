
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public int attackDamage = 10;
    public float attackRange = 1.5f;
    public float attackRadius = 1f;
    public float attackCooldown = 0.8f;
    public float hitDelay = 0.25f;

    [Header("타격 이펙트")]
    public GameObject hitEffectPrefab;
    public float hitEffectHeight = 0.5f;

    [Header("검 궤적 이펙트")]
    public TrailRenderer swordTrail;
    public float trailStartDelay = 0.05f;
    public float trailDuration = 0.25f;

    [Header("공격 및 사망 사운드")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    private Animator animator;
    private AudioSource audioSource;
    private float nextAttackTime;
    private Coroutine trailCoroutine;
    private PlayerHealth health;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<PlayerHealth>();

        // 게임 시작 시 검 궤적 끄기
        if (swordTrail != null)
        {
            swordTrail.emitting = false;
            swordTrail.Clear();
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // 스페이스바 공격
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || (health != null && health.IsDead))
            return;

        // 공격 쿨타임 확인
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        // 공격 애니메이션 실행
        animator.SetTrigger("Attack");

        Debug.Log("플레이어 검 공격!");

        // 검 궤적 실행
        if (swordTrail != null)
        {
            if (trailCoroutine != null)
            {
                StopCoroutine(trailCoroutine);
            }

            trailCoroutine = StartCoroutine(PlaySwordTrail());
        }

        // 슬라임 타격 판정
        Invoke(nameof(CheckAttackHit), hitDelay);
    }

    // 검 궤적 재생
    IEnumerator PlaySwordTrail()
    {
        swordTrail.emitting = false;
        swordTrail.Clear();

        yield return new WaitForSeconds(trailStartDelay);

        swordTrail.Clear();
        swordTrail.emitting = true;

        yield return new WaitForSeconds(trailDuration);

        swordTrail.emitting = false;
        trailCoroutine = null;
    }

    // 슬라임 타격 판정
    void CheckAttackHit()
    {
        if (!isActiveAndEnabled || (health != null && health.IsDead))
            return;

        Vector3 attackCenter =
            transform.position
            + transform.forward * attackRange
            + Vector3.up * 0.5f;

        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            attackRadius
        );

        SlimeHealth targetSlime = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            SlimeHealth slime =
                hit.GetComponentInParent<SlimeHealth>();

            if (slime == null)
                continue;

            // 플레이어 앞쪽인지 확인
            Vector3 toSlime =
                slime.transform.position - transform.position;

            toSlime.y = 0f;

            if (Vector3.Dot(
                transform.forward,
                toSlime
            ) <= 0f)
            {
                continue;
            }

            // 가장 가까운 슬라임 검색
            float distance = toSlime.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetSlime = slime;
            }
        }

        // 공격 성공
        if (targetSlime != null)
        {
            // 슬라임의 위치와 이름 저장
            Vector3 hitPosition =
                targetSlime.transform.position
                + Vector3.up * hitEffectHeight;

            string slimeName = targetSlime.name;

            // 이번 공격으로 슬라임이 죽는지 확인
            bool willDie =
                targetSlime.CurrentHealth > 0 &&
                targetSlime.CurrentHealth <= attackDamage;

            // 슬라임에게 데미지 적용
            targetSlime.TakeDamage(attackDamage);

            // 피격 파티클 생성
            if (hitEffectPrefab != null)
            {
                Instantiate(
                    hitEffectPrefab,
                    hitPosition,
                    Quaternion.identity
                );
            }

            // 공격 및 사망 사운드 재생
            if (audioSource != null)
            {
                AudioClip selectedSound =
                    willDie ? deathSound : hitSound;

                if (selectedSound != null)
                {
                    audioSource.PlayOneShot(
                        selectedSound,
                        soundVolume
                    );
                }
            }

            if (willDie)
            {
                Debug.Log(slimeName + " 사망 사운드 재생!");
            }
            else
            {
                Debug.Log(slimeName + " 타격 사운드 재생!");
            }

            Debug.Log(slimeName + " 공격 성공!");
        }
        else
        {
            Debug.Log("공격 범위에 슬라임이 없습니다.");
        }
    }

    void OnDisable()
    {
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }
        // 남아 있는 타격 판정 취소
        CancelInvoke(nameof(CheckAttackHit));

        // 검 궤적 정리
        if (swordTrail != null)
        {
            swordTrail.emitting = false;
            swordTrail.Clear();
        }
    }

    // Scene 창에서 공격 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 attackCenter =
            transform.position
            + transform.forward * attackRange
            + Vector3.up * 0.5f;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );
    }
}
