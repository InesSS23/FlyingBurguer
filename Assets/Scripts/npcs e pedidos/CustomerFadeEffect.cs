using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomerFadeEffect : MonoBehaviour
{
    private class MaterialState
    {
        public Material material;

        public int renderQueue;
        public string[] shaderKeywords;

        public bool hasBaseColor;
        public Color baseColor;

        public bool hasColor;
        public Color color;

        public bool hasSurface;
        public float surface;

        public bool hasMode;
        public float mode;

        public bool hasSrcBlend;
        public float srcBlend;

        public bool hasDstBlend;
        public float dstBlend;

        public bool hasZWrite;
        public float zWrite;
    }

    private readonly List<Material> materials = new List<Material>();
    private readonly Dictionary<Material, MaterialState> originalStates = new Dictionary<Material, MaterialState>();

    private Coroutine currentFadeRoutine;

    public void FadeIn(float duration)
    {
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        currentFadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration, true));
    }

    public IEnumerator FadeOutRoutine(float duration)
    {
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        yield return FadeRoutine(1f, 0f, duration, false);
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, bool restoreAtEnd)
    {
        PrepareMaterialsForFade();

        if (duration <= 0f)
        {
            SetAlpha(endAlpha);

            if (restoreAtEnd)
            {
                RestoreOriginalMaterials();
            }

            currentFadeRoutine = null;
            yield break;
        }

        SetAlpha(startAlpha);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(endAlpha);

        if (restoreAtEnd)
        {
            RestoreOriginalMaterials();
        }

        currentFadeRoutine = null;
    }

    private void PrepareMaterialsForFade()
    {
        materials.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            Material[] rendererMaterials = renderer.materials;

            for (int j = 0; j < rendererMaterials.Length; j++)
            {
                Material material = rendererMaterials[j];

                if (material == null)
                    continue;

                if (!materials.Contains(material))
                {
                    materials.Add(material);
                }

                if (!originalStates.ContainsKey(material))
                {
                    SaveOriginalState(material);
                }

                MakeMaterialTransparent(material);
            }
        }
    }

    private void SaveOriginalState(Material material)
    {
        MaterialState state = new MaterialState();

        state.material = material;
        state.renderQueue = material.renderQueue;
        state.shaderKeywords = material.shaderKeywords;

        state.hasBaseColor = material.HasProperty("_BaseColor");
        if (state.hasBaseColor)
        {
            state.baseColor = material.GetColor("_BaseColor");
        }

        state.hasColor = material.HasProperty("_Color");
        if (state.hasColor)
        {
            state.color = material.GetColor("_Color");
        }

        state.hasSurface = material.HasProperty("_Surface");
        if (state.hasSurface)
        {
            state.surface = material.GetFloat("_Surface");
        }

        state.hasMode = material.HasProperty("_Mode");
        if (state.hasMode)
        {
            state.mode = material.GetFloat("_Mode");
        }

        state.hasSrcBlend = material.HasProperty("_SrcBlend");
        if (state.hasSrcBlend)
        {
            state.srcBlend = material.GetFloat("_SrcBlend");
        }

        state.hasDstBlend = material.HasProperty("_DstBlend");
        if (state.hasDstBlend)
        {
            state.dstBlend = material.GetFloat("_DstBlend");
        }

        state.hasZWrite = material.HasProperty("_ZWrite");
        if (state.hasZWrite)
        {
            state.zWrite = material.GetFloat("_ZWrite");
        }

        originalStates.Add(material, state);
    }

    private void MakeMaterialTransparent(Material material)
    {
        // URP Lit
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        // Standard Shader antigo
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHATEST_ON");

        material.renderQueue = (int)RenderQueue.Transparent;
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < materials.Count; i++)
        {
            Material material = materials[i];

            if (material == null)
                continue;

            MaterialState state = null;

            if (originalStates.ContainsKey(material))
            {
                state = originalStates[material];
            }

            if (material.HasProperty("_BaseColor"))
            {
                Color color = state != null && state.hasBaseColor ? state.baseColor : material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                Color color = state != null && state.hasColor ? state.color : material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (KeyValuePair<Material, MaterialState> pair in originalStates)
        {
            Material material = pair.Key;
            MaterialState state = pair.Value;

            if (material == null || state == null)
                continue;

            material.renderQueue = state.renderQueue;

            if (state.shaderKeywords != null)
            {
                material.shaderKeywords = state.shaderKeywords;
            }

            if (state.hasBaseColor && material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", state.baseColor);
            }

            if (state.hasColor && material.HasProperty("_Color"))
            {
                material.SetColor("_Color", state.color);
            }

            if (state.hasSurface && material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", state.surface);
            }

            if (state.hasMode && material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", state.mode);
            }

            if (state.hasSrcBlend && material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", state.srcBlend);
            }

            if (state.hasDstBlend && material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", state.dstBlend);
            }

            if (state.hasZWrite && material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", state.zWrite);
            }
        }
    }
}