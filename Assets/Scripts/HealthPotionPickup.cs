using UnityEngine;
using UnityEngine.InputSystem;

public sealed class HealthPotionPickup : MonoBehaviour
{
    private int healAmount;
    private PlayerHealth player;
    private Vector3 origin;
    private Material liquidMaterial;
    private Material capMaterial;
    private Material markMaterial;
    private bool collected;
    private static int lastPickupFrame = -1;
    private GUIStyle promptStyle;

    private bool InPickupRange()
    {
        if (player == null || player.IsDead) return false;
        Vector3 distance = player.transform.position - origin;
        if (Mathf.Abs(distance.y) > 1.5f) return false;
        distance.y = 0f;
        return distance.sqrMagnitude <= 1.2f * 1.2f;
    }

    public static HealthPotionPickup Spawn(Vector3 position, int amount)
    {
        var root = new GameObject("Bonus HP Potion");
        root.transform.position = position + Vector3.up * 0.4f;
        var pickup = root.AddComponent<HealthPotionPickup>();
        pickup.healAmount = Mathf.Max(1, amount);
        pickup.origin = root.transform.position;
        pickup.player = FindFirstObjectByType<PlayerHealth>();

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        pickup.liquidMaterial = new Material(shader) { color = new Color(0.95f, 0.05f, 0.18f) };
        pickup.capMaterial = new Material(shader) { color = new Color(0.8f, 0.57f, 0.22f) };
        pickup.markMaterial = new Material(shader) { color = Color.white };
        pickup.AddPart("Bottle", PrimitiveType.Sphere, Vector3.zero,
            new Vector3(0.4f, 0.45f, 0.4f), pickup.liquidMaterial);
        pickup.AddPart("Cap", PrimitiveType.Cylinder, new Vector3(0, 0.25f, 0),
            new Vector3(0.18f, 0.055f, 0.18f), pickup.capMaterial);
        pickup.AddPart("Cross Vertical", PrimitiveType.Cube, new Vector3(0, 0, -0.2f),
            new Vector3(0.06f, 0.23f, 0.025f), pickup.markMaterial);
        pickup.AddPart("Cross Horizontal", PrimitiveType.Cube, new Vector3(0, 0, -0.2f),
            new Vector3(0.2f, 0.06f, 0.025f), pickup.markMaterial);
        return pickup;
    }

    private void AddPart(string partName, PrimitiveType shape, Vector3 position,
        Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = partName;
        part.transform.SetParent(transform, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        var collider = part.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(collider);
    }

    void Update()
    {
        if (collected || Time.timeScale <= 0f)
            return;
        transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.08f);
        transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
        if (player == null || player.IsDead || player.CurrentHealth >= player.MaxHealth)
            return;

        if (!InPickupRange() || Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame || lastPickupFrame == Time.frameCount)
            return;

        lastPickupFrame = Time.frameCount;
        collected = true;
        player.Heal(healAmount);
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (collected || Time.timeScale <= 0f || !InPickupRange() || Camera.main == null)
            return;
        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        if (screen.z <= 0f) return;
        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.box)
            {
                font = Resources.Load<Font>("Fonts/NanumGothic-Regular"), fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
        }
        GUI.Box(new Rect(screen.x - 110, Screen.height - screen.y - 34, 220, 34),
            player.CurrentHealth >= player.MaxHealth ? "HP가 가득 찼습니다" : $"[E] 물약 먹기 · HP +{healAmount}", promptStyle);
    }

    void OnDestroy()
    {
        if (liquidMaterial != null) Destroy(liquidMaterial);
        if (capMaterial != null) Destroy(capMaterial);
        if (markMaterial != null) Destroy(markMaterial);
    }
}
