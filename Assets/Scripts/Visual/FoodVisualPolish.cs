using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Melhora apenas os materiais dos restantes alimentos. O pao conserva
/// integralmente o modelo e o material originais do projeto.
/// </summary>
public sealed class FoodVisualPolish : MonoBehaviour
{
    private enum FoodType
    {
        None,
        Cheese,
        Lettuce,
        Tomato,
        RawMeat,
        CookedMeat
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Level"))
            return;

        if (FindFirstObjectByType<FoodVisualPolish>() != null)
            return;

        GameObject host = new GameObject("Food Visual Polish");
        host.AddComponent<FoodVisualPolish>();
    }

    private IEnumerator Start()
    {
        WaitForSeconds wait = new WaitForSeconds(0.75f);

        while (true)
        {
            PolishNewFoodObjects();
            yield return wait;
        }
    }

    private static void PolishNewFoodObjects()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < renderers.Length; i++)
        {
            Transform root = FindFoodRoot(renderers[i].transform, out FoodType type);
            if (root == null || root.GetComponent<FoodVisualPolishMarker>() != null)
                continue;

            root.gameObject.AddComponent<FoodVisualPolishMarker>();
            ApplyMaterialFinish(root, type);
        }
    }

    private static Transform FindFoodRoot(Transform current, out FoodType type)
    {
        type = FoodType.None;

        for (int depth = 0; current != null && depth < 8; depth++, current = current.parent)
        {
            string objectName = current.name.ToLowerInvariant();

            if (objectName.Contains("cheesevisual"))
                type = FoodType.Cheese;
            else if (objectName.Contains("lettucevisual") || objectName.Contains("lettucefixed"))
                type = FoodType.Lettuce;
            else if (objectName.Contains("tomatovisual"))
                type = FoodType.Tomato;
            else if (objectName.Contains("rawmeatvisual"))
                type = FoodType.RawMeat;
            else if (objectName.Contains("cookedmeatvisual"))
                type = FoodType.CookedMeat;

            if (type != FoodType.None)
                return current;
        }

        return null;
    }

    private static void ApplyMaterialFinish(Transform root, FoodType type)
    {
        Color color = GetFoodColor(type);
        Renderer[] foodRenderers = root.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < foodRenderers.Length; i++)
        {
            Material[] materials = foodRenderers[i].materials;

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null)
                    continue;

                material.name = type + " Cartoon Finish";

                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", color);
                else if (material.HasProperty("_Color"))
                    material.SetColor("_Color", color);

                if (material.HasProperty("_Smoothness"))
                    material.SetFloat("_Smoothness", type == FoodType.RawMeat ? 0.38f : 0.24f);

                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", 0f);

                if (material.HasProperty("_SpecularHighlights"))
                    material.SetFloat("_SpecularHighlights", 1f);
            }

            foodRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            foodRenderers[i].receiveShadows = true;
        }
    }

    private static Color GetFoodColor(FoodType type)
    {
        switch (type)
        {
            case FoodType.Cheese:
                return new Color(1f, 0.72f, 0.12f, 1f);
            case FoodType.Lettuce:
                return new Color(0.31f, 0.72f, 0.20f, 1f);
            case FoodType.Tomato:
                return new Color(0.91f, 0.18f, 0.10f, 1f);
            case FoodType.RawMeat:
                return new Color(0.78f, 0.16f, 0.19f, 1f);
            case FoodType.CookedMeat:
                return new Color(0.36f, 0.12f, 0.055f, 1f);
            default:
                return Color.white;
        }
    }
}

internal sealed class FoodVisualPolishMarker : MonoBehaviour
{
}
