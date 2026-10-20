using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlayerDeathMenu : MonoBehaviour
{
    public bool IsGameOver { get; private set; }
    private bool restarting;
    private GUIStyle titleStyle;
    private GUIStyle messageStyle;
    private GUIStyle buttonStyle;
    private Font menuFont;
    private GUIStyle panelStyle;
    private GUIStyle headerStyle;

    private static GUIStyle SpriteStyle(UnityEngine.UI.Image image)
    {
        var style = new GUIStyle();
        if (image == null || image.sprite == null) return style;
        Sprite sprite = image.sprite;
        style.normal.background = sprite.texture;
        Vector4 border = sprite.border;
        style.border = new RectOffset((int)border.x, (int)border.z, (int)border.w, (int)border.y);
        return style;
    }

    public void Show()
    {
        IsGameOver = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var quest = FindFirstObjectByType<QuestManager>();
        if (quest != null)
        {
            quest.StopAllCoroutines();
            if (quest.questCompletePanel != null) quest.questCompletePanel.SetActive(false);
        }
    }

    public void RestartGame()
    {
        if (restarting || IsGameOver) return;
        restarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
        var attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.enabled = false;
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
    }

    void LateUpdate()
    {
        if (!restarting) Time.timeScale = 0f;
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            menuFont = Resources.Load<Font>("Fonts/NanumGothic-Regular");
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = menuFont, fontSize = 36, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.91f, 0.66f);
            messageStyle = new GUIStyle(titleStyle) { fontSize = 20, fontStyle = FontStyle.Normal };
            messageStyle.normal.textColor = new Color(0.29f, 0.18f, 0.08f);
            panelStyle = new GUIStyle(GUI.skin.box);
            headerStyle = new GUIStyle(GUI.skin.box);
            buttonStyle = new GUIStyle(GUI.skin.button);
            var quest = FindFirstObjectByType<QuestManager>();
            if (quest != null && quest.questCompletePanel != null)
            {
                foreach (var image in quest.questCompletePanel.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                {
                    if (image.name == "PopupWindow") panelStyle = SpriteStyle(image);
                    if (image.name == "Popup_top") headerStyle = SpriteStyle(image);
                    if (image.name == "PlayAgainButton") buttonStyle = SpriteStyle(image);
                }
            }
            buttonStyle.font = menuFont;
            buttonStyle.fontSize = 22;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.contentOffset = new Vector2(0f, -8f);
            buttonStyle.normal.textColor = new Color(0.18f, 0.24f, 0.05f);
            buttonStyle.hover.background = buttonStyle.normal.background;
            buttonStyle.hover.textColor = new Color(0.3f, 0.36f, 0.08f);
            buttonStyle.active.background = buttonStyle.normal.background;
            buttonStyle.active.textColor = Color.white;
        }

        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;
        GUI.depth = -1000;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 960f, Screen.height / 540f, 1f));
        GUI.color = new Color(0.04f, 0.06f, 0.02f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, 960, 540), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Box(new Rect(220, 140, 520, 260), GUIContent.none, panelStyle);
        GUI.Box(new Rect(242, 158, 476, 76), GUIContent.none, headerStyle);
        GUI.Label(new Rect(255, 164, 450, 58), IsGameOver ? "GAME OVER" : "플레이어 사망", titleStyle);
        GUI.Label(new Rect(250, 252, 460, 40),
            IsGameOver ? "게임이 종료되었습니다." : "게임을 다시 시작하시겠습니까?", messageStyle);
        if (!IsGameOver)
        {
            if (GUI.Button(new Rect(270, 315, 195, 58), "다시 시작", buttonStyle)) RestartGame();
            if (GUI.Button(new Rect(495, 315, 195, 58), "종료", buttonStyle)) EndGame();
        }
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    void OnDestroy()
    {
        // The bundled font is a shared Resources asset.
    }
}
