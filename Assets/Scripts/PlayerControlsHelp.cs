using UnityEngine;

public sealed class PlayerControlsHelp : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    private float previousTimeScale;
    private GUIStyle panel;
    private GUIStyle header;
    private GUIStyle button;
    private GUIStyle title;
    private GUIStyle text;

    public void Open()
    {
        if (IsOpen || Time.timeScale <= 0f || GetComponent<PlayerDeathMenu>() != null) return;
        previousTimeScale = Time.timeScale;
        IsOpen = true;
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (GetComponent<PlayerDeathMenu>() == null) Time.timeScale = previousTimeScale;
    }

    private static GUIStyle ImageStyle(UnityEngine.UI.Image image)
    {
        Vector4 b = image.sprite.border;
        var style = new GUIStyle { border = new RectOffset((int)b.x, (int)b.z, (int)b.w, (int)b.y) };
        style.normal.background = image.sprite.texture;
        return style;
    }

    void OnGUI()
    {
        if (GetComponent<PlayerDeathMenu>() != null) { IsOpen = false; return; }
        if (text == null)
        {
            var font = Resources.Load<Font>("Fonts/NanumGothic-Regular");
            title = new GUIStyle(GUI.skin.label) { font = font, fontSize = 28,
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            title.normal.textColor = new Color(1f, 0.91f, 0.66f);
            text = new GUIStyle(title) { fontSize = 19, fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft };
            text.normal.textColor = new Color(0.29f, 0.18f, 0.08f);
            panel = new GUIStyle(GUI.skin.box);
            header = new GUIStyle(GUI.skin.box);
            button = new GUIStyle(GUI.skin.button);
            var quest = FindFirstObjectByType<QuestManager>();
            if (quest != null && quest.questCompletePanel != null)
                foreach (var image in quest.questCompletePanel.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                {
                    if (image.sprite == null) continue;
                    if (image.name == "PopupWindow") panel = ImageStyle(image);
                    if (image.name == "Popup_top") header = ImageStyle(image);
                    if (image.name == "PlayAgainButton") button = ImageStyle(image);
                }
            button.font = font;
            button.fontSize = 22;
            button.fontStyle = FontStyle.Bold;
            button.alignment = TextAnchor.MiddleCenter;
            button.contentOffset = new Vector2(0, -6);
            button.normal.textColor = text.normal.textColor;
            button.hover.background = button.normal.background;
            button.active.background = button.normal.background;
        }
        var oldMatrix = GUI.matrix;
        var oldColor = GUI.color;
        int oldDepth = GUI.depth;
        GUI.depth = -500;
        // Uniform scaling keeps the question-mark button square at all aspect ratios.
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960f * scale) / 2f,
            (Screen.height - 540f * scale) / 2f, 0), Quaternion.identity, Vector3.one * scale);
        GUI.color = Color.white;
        if (!IsOpen)
        {
            if (GUI.Button(new Rect(902, 66, 42, 42), new GUIContent("?", "조작 안내"), button)) Open();
        }
        else
        {
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(-960, -540, 2880, 1620), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Box(new Rect(210, 45, 540, 450), GUIContent.none, panel);
            GUI.Box(new Rect(232, 63, 496, 66), GUIContent.none, header);
            GUI.Label(new Rect(245, 70, 470, 48), "?  조작 안내", title);
            string[] rows = { "W A S D     이동", "Shift          누르고 달리기", "Space        바라보는 방향으로 공격",
                "E                 가까운 물약 먹기 · HP +50", Application.platform == RuntimePlatform.WindowsPlayer
                    ? "ESC           프로그램 종료" : "ESC           즉시 종료 · GAME OVER" };
            for (int i = 0; i < rows.Length; i++)
                GUI.Label(new Rect(252, 147 + i * 42, 460, 36), rows[i], text);
            GUI.Label(new Rect(252, 358, 460, 50), "레벨 1~3을 완료하고 보스를 처치하세요.\n안내창을 닫으면 게임이 계속됩니다.", new GUIStyle(text) { fontSize = 16 });
            if (GUI.Button(new Rect(390, 420, 180, 52), "닫기", button)) Close();
        }
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
        GUI.depth = oldDepth;
    }
}
