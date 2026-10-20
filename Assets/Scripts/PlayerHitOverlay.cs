using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Separate renderers keep the character's original materials untouched.
[DisallowMultipleComponent]
public sealed class PlayerHitOverlay : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.25f;
    [SerializeField] private Color hitColor = new Color(1f, 0f, 0f, 0.45f);

    private readonly List<Renderer> sources = new List<Renderer>();
    private readonly List<Renderer> overlays = new List<Renderer>();
    private Material overlayMaterial;
    private float remaining;

    void Awake()
    {
        Shader shader = Resources.Load<Shader>("PlayerHitOverlay");
        if (shader == null)
        {
            Debug.LogError("PlayerHitOverlay shader is missing.", this);
            return;
        }
        overlayMaterial = new Material(shader);
        foreach (Renderer source in GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh;
            if (source is SkinnedMeshRenderer skin)
                mesh = skin.sharedMesh;
            else if (source is MeshRenderer && source.TryGetComponent(out MeshFilter filter))
                mesh = filter.sharedMesh;
            else
                continue;
            if (mesh == null)
                continue;

            var child = new GameObject("HitOverlay");
            child.layer = source.gameObject.layer;
            child.transform.SetParent(source.transform, false);
            Renderer overlay;
            if (source is SkinnedMeshRenderer skinned)
            {
                var copy = child.AddComponent<SkinnedMeshRenderer>();
                copy.sharedMesh = mesh;
                copy.bones = skinned.bones;
                copy.rootBone = skinned.rootBone;
                copy.localBounds = skinned.localBounds;
                copy.quality = skinned.quality;
                overlay = copy;
            }
            else
            {
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                overlay = child.AddComponent<MeshRenderer>();
            }
            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++)
                materials[i] = overlayMaterial;
            overlay.sharedMaterials = materials;
            overlay.shadowCastingMode = ShadowCastingMode.Off;
            overlay.receiveShadows = false;
            overlay.enabled = false;
            sources.Add(source);
            overlays.Add(overlay);
        }
    }

    public void Play()
    {
        remaining = Mathf.Max(0.01f, duration);
        UpdateOverlay();
    }

    void LateUpdate()
    {
        if (remaining <= 0f)
            return;
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        UpdateOverlay();
    }

    void UpdateOverlay()
    {
        if (overlayMaterial == null)
            return;
        Color color = hitColor;
        color.a *= Mathf.Clamp01(remaining / Mathf.Max(0.01f, duration));
        overlayMaterial.SetColor("_HitColor", color);
        for (int i = 0; i < overlays.Count; i++)
        {
            if (overlays[i] == null || sources[i] == null)
                continue;
            overlays[i].enabled = remaining > 0f && sources[i].enabled;
            if (sources[i] is SkinnedMeshRenderer skin && overlays[i] is SkinnedMeshRenderer copy)
                for (int j = 0; j < skin.sharedMesh.blendShapeCount; j++)
                    copy.SetBlendShapeWeight(j, skin.GetBlendShapeWeight(j));
        }
    }

    void OnDisable()
    {
        remaining = 0f;
        foreach (Renderer overlay in overlays)
            if (overlay != null) overlay.enabled = false;
    }

    void OnDestroy()
    {
        foreach (Renderer overlay in overlays)
            if (overlay != null) Destroy(overlay.gameObject);
        if (overlayMaterial != null) Destroy(overlayMaterial);
    }
}
