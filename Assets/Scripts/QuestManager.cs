using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public SlimeSpawner slimeSpawner;
    public float nextLevelDelay = 1.2f;

    [Header("Quest UI")]
    public TMP_Text countText;
    public TMP_Text levelText;
    public RectTransform progressFill;

    [Header("Quest Complete Popup")]
    public GameObject questCompletePanel;
    public TMP_Text completeTitleText;
    public TMP_Text completeMessageText;

    private int targetKills;
    private int currentKills;
    private float maxFillWidth;
    private bool isLevelTransitioning;
    private bool isBossFight;
    private bool isGameComplete;
    private SlimeHealth activeBoss;

    void Awake()
    {
        if (progressFill != null)
        {
            maxFillWidth = progressFill.rect.width;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (questCompletePanel != null)
        {
            questCompletePanel.SetActive(false);
        }

        StartLevel(1);
    }

    // 새 레벨 시작
    void StartLevel(int level)
    {
        currentLevel = level;
        currentKills = 0;
        isLevelTransitioning = false;

        if (slimeSpawner == null)
        {
            Debug.LogError(
                "SlimeSpawner가 연결되지 않았습니다."
            );
            return;
        }

        targetKills = slimeSpawner.SpawnLevel(level);

        if (targetKills <= 0)
        {
            Debug.LogError(
                "슬라임이 생성되지 않았습니다. " +
                "Ground Layer와 스폰 영역을 확인하세요."
            );
            return;
        }

        UpdateQuestUI();

        Debug.Log(
            "LEVEL " + currentLevel +
            " START! " + targetKills + " slimes."
        );
    }

    // 슬라임 사망 시 호출
    public void AddKill(SlimeHealth defeatedSlime = null)
    {
        if (isGameComplete || isLevelTransitioning || targetKills <= 0)
            return;

        if (isBossFight && defeatedSlime != activeBoss)
            return;

        currentKills = Mathf.Min(
            currentKills + 1,
            targetKills
        );

        UpdateQuestUI();

        Debug.Log(
            "LEVEL " + currentLevel +
            " : Slimes Defeated " +
            currentKills + "/" + targetKills
        );

        if (currentKills >= targetKills)
        {
            isLevelTransitioning = true;
            StartCoroutine(FinishLevel());
        }
    }

    void UpdateQuestUI()
    {
        if (countText != null)
        {
            countText.text =
                (isBossFight ? "Boss Defeated: " : "Slimes Defeated: ") +
                currentKills + " / " + targetKills;
        }

        if (levelText != null)
        {
            levelText.text = isBossFight ? "FINAL BOSS" : "LEVEL " + currentLevel;
        }

        if (progressFill != null && targetKills > 0)
        {
            float ratio =
                (float)currentKills / targetKills;

            progressFill.gameObject.SetActive(
                currentKills > 0
            );

            progressFill.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                maxFillWidth * ratio
            );
        }
    }

    IEnumerator FinishLevel()
    {
        // 기존 사망 애니메이션과 파티클 시간 확보
        yield return new WaitForSeconds(nextLevelDelay);

        if (isBossFight)
        {
            CompleteQuest();
        }
        else if (currentLevel < Mathf.Min(3, slimeSpawner.levels.Length))
        {
            StartLevel(currentLevel + 1);
        }
        else
        {
            StartBossFight();
        }
    }

    void StartBossFight()
    {
        activeBoss = slimeSpawner.SpawnBoss();
        if (activeBoss == null)
        {
            Debug.LogError("보스 생성 실패: 프리팹과 스폰 영역을 확인하세요.");
            return;
        }

        activeBoss.questManager = this;
        isBossFight = true;
        currentKills = 0;
        targetKills = 1;
        isLevelTransitioning = false;
        UpdateQuestUI();
    }

    void CompleteQuest()
    {
        isGameComplete = true;
        Debug.Log("ALL LEVELS COMPLETE!");

        if (completeTitleText != null)
        {
            completeTitleText.text = "GAME CLEAR!";
        }

        if (completeMessageText != null)
        {
            completeMessageText.text =
                "All 3 levels cleared! Final boss defeated!";
        }

        if (questCompletePanel != null)
        {
            questCompletePanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
