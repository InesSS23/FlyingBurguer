using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InteriorWallInstaller
{
    private const string TextureResourcePath = "Textures/Wall_White_Brick";
    private const string WoodTextureResourcePath = "Textures/Ceiling_Light_Wood_Planks";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Level"))
            return;

        Texture2D wallTexture = Resources.Load<Texture2D>(TextureResourcePath);
        Texture2D woodTexture = Resources.Load<Texture2D>(WoodTextureResourcePath);
        GameObject roulote = GameObject.Find("Cenario_Rolote_Demo");

        if (wallTexture == null || woodTexture == null || roulote == null)
        {
            Debug.LogWarning("Nao foi possivel aplicar a textura nas paredes da roulote.");
            return;
        }

        Renderer[] renderers = roulote.GetComponentsInChildren<Renderer>(true);
        int changedMaterialCount = 0;

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer targetRenderer = renderers[rendererIndex];
            Material[] materials = targetRenderer.materials;
            bool rendererChanged = false;

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material sourceMaterial = materials[materialIndex];
                if (sourceMaterial == null || !IsWallMaterial(targetRenderer.name, sourceMaterial))
                    continue;

                Material wallMaterial = new Material(sourceMaterial);
                wallMaterial.name = sourceMaterial.name + " - Tijolo Interior";
                wallMaterial.mainTexture = wallTexture;
                wallMaterial.mainTextureScale = Vector2.one;
                wallMaterial.color = Color.white;

                if (wallMaterial.HasProperty("_Smoothness"))
                    wallMaterial.SetFloat("_Smoothness", 0.08f);

                materials[materialIndex] = wallMaterial;
                rendererChanged = true;
                changedMaterialCount++;
            }

            if (rendererChanged)
            {
                targetRenderer.materials = materials;
                ApplyWallUVMapping(targetRenderer, woodTexture);
            }
        }

        if (changedMaterialCount == 0)
            Debug.LogWarning("Nao encontrei o material das paredes no modelo da roulote.");
    }

    private static void ApplyWallUVMapping(Renderer targetRenderer, Texture2D woodTexture)
    {
        MeshFilter meshFilter = targetRenderer.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh sourceMesh = meshFilter.sharedMesh;
        Mesh mappedMesh = Object.Instantiate(sourceMesh);
        mappedMesh.name = sourceMesh.name + " - Brick UV";

        Vector3[] vertices = mappedMesh.vertices;
        Vector3[] normals = mappedMesh.normals;
        Vector2[] wallUVs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = targetRenderer.transform.TransformPoint(vertices[i]);
            Vector3 localNormal = i < normals.Length ? normals[i] : Vector3.forward;
            Vector3 worldNormal = targetRenderer.transform.TransformDirection(localNormal).normalized;

            if (Mathf.Abs(worldNormal.y) > 0.7f)
            {
                wallUVs[i] = new Vector2(worldVertex.x / 4f, worldVertex.z / 4f);
            }
            else if (Mathf.Abs(worldNormal.x) > Mathf.Abs(worldNormal.z))
            {
                wallUVs[i] = new Vector2(worldVertex.z / 4f, worldVertex.y / 4f);
            }
            else
            {
                wallUVs[i] = new Vector2(worldVertex.x / 4f, worldVertex.y / 4f);
            }
        }

        mappedMesh.uv = wallUVs;
        SeparateCeilingTriangles(mappedMesh, targetRenderer, woodTexture);
        meshFilter.sharedMesh = mappedMesh;
    }

    private static void SeparateCeilingTriangles(
        Mesh mesh,
        Renderer targetRenderer,
        Texture2D woodTexture
    )
    {
        Material[] currentMaterials = targetRenderer.materials;
        List<Material> finalMaterials = new List<Material>(currentMaterials);
        List<int[]> ceilingTriangleGroups = new List<int[]>();
        List<int> sourceMaterialIndices = new List<int>();
        Dictionary<int, int[]> remainingTriangles = new Dictionary<int, int[]>();
        Vector3[] vertices = mesh.vertices;
        int originalSubMeshCount = mesh.subMeshCount;

        for (int subMesh = 0; subMesh < originalSubMeshCount; subMesh++)
        {
            if (subMesh >= currentMaterials.Length)
                continue;

            Material material = currentMaterials[subMesh];
            if (material == null || !Normalize(material.name).Contains("tijolo interior"))
                continue;

            int[] triangles = mesh.GetTriangles(subMesh);
            List<int> wallTriangles = new List<int>(triangles.Length);
            List<int> ceilingTriangles = new List<int>();

            for (int triangle = 0; triangle < triangles.Length; triangle += 3)
            {
                int a = triangles[triangle];
                int b = triangles[triangle + 1];
                int c = triangles[triangle + 2];
                Vector3 localFaceNormal = Vector3.Cross(
                    vertices[b] - vertices[a],
                    vertices[c] - vertices[a]
                ).normalized;
                Vector3 worldFaceNormal = targetRenderer.transform.TransformDirection(localFaceNormal).normalized;
                List<int> destination = Mathf.Abs(worldFaceNormal.y) > 0.82f
                    ? ceilingTriangles
                    : wallTriangles;

                destination.Add(a);
                destination.Add(b);
                destination.Add(c);
            }

            remainingTriangles[subMesh] = wallTriangles.ToArray();

            if (ceilingTriangles.Count > 0)
            {
                ceilingTriangleGroups.Add(ceilingTriangles.ToArray());
                sourceMaterialIndices.Add(subMesh);
            }
        }

        mesh.subMeshCount = originalSubMeshCount + ceilingTriangleGroups.Count;

        foreach (KeyValuePair<int, int[]> entry in remainingTriangles)
            mesh.SetTriangles(entry.Value, entry.Key, false);

        for (int i = 0; i < ceilingTriangleGroups.Count; i++)
        {
            int newSubMesh = originalSubMeshCount + i;
            mesh.SetTriangles(ceilingTriangleGroups[i], newSubMesh, false);

            Material woodMaterial = new Material(currentMaterials[sourceMaterialIndices[i]]);
            woodMaterial.name = "Teto - Madeira Clara";
            woodMaterial.mainTexture = woodTexture;
            woodMaterial.mainTextureScale = new Vector2(1.5f, 1.5f);
            woodMaterial.color = Color.white;

            if (woodMaterial.HasProperty("_Smoothness"))
                woodMaterial.SetFloat("_Smoothness", 0.12f);

            finalMaterials.Add(woodMaterial);
        }

        targetRenderer.materials = finalMaterials.ToArray();
        mesh.RecalculateBounds();
    }

    private static bool IsWallMaterial(string rendererName, Material material)
    {
        string normalizedRenderer = Normalize(rendererName);
        string normalizedMaterial = material == null ? string.Empty : Normalize(material.name);

        return normalizedRenderer.Contains("paredes")
            || normalizedMaterial.Contains("paredes")
            || normalizedMaterial.Contains("camiao");
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        string decomposed = value.Normalize(NormalizationForm.FormD);
        StringBuilder result = new StringBuilder(decomposed.Length);

        for (int i = 0; i < decomposed.Length; i++)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(decomposed[i]);
            if (category != UnicodeCategory.NonSpacingMark)
                result.Append(char.ToLowerInvariant(decomposed[i]));
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
