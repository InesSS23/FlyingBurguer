using UnityEngine;
using UnityEngine.SceneManagement;

public static class CheckerFloorInstaller
{
    private const string FloorObjectName = "Chao_Xadrez_Coral";
    private const string TextureResourcePath = "Textures/Floor_Checker_Coral_Cream";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Level"))
            return;

        if (GameObject.Find(FloorObjectName) != null)
            return;

        BoxCollider floorCollider = FindFloorCollider();
        Texture2D floorTexture = Resources.Load<Texture2D>(TextureResourcePath);

        if (floorCollider == null || floorTexture == null)
        {
            Debug.LogWarning("Nao foi possivel criar o chao em xadrez.");
            return;
        }

        Bounds bounds = floorCollider.bounds;
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = FloorObjectName;
        floor.transform.position = new Vector3(bounds.center.x, bounds.max.y + 0.015f, bounds.center.z);
        floor.transform.localScale = new Vector3(bounds.size.x / 10f, 1f, bounds.size.z / 10f);

        Collider generatedCollider = floor.GetComponent<Collider>();
        if (generatedCollider != null)
            Object.Destroy(generatedCollider);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = "Chao Xadrez Coral (Runtime)";
        material.mainTexture = floorTexture;
        // A textura usa uma grelha par de 6x6. Uma repeticao a cada 6 metros
        // conserva azulejos de 1 metro e evita que duas cores iguais se juntem.
        material.mainTextureScale = new Vector2(bounds.size.x / 6f, bounds.size.z / 6f);
        material.color = Color.white;

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.16f);

        floor.GetComponent<MeshRenderer>().material = material;
    }

    private static BoxCollider FindFloorCollider()
    {
        BoxCollider[] colliders = Object.FindObjectsByType<BoxCollider>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].name.Trim().Equals("FloorCollider", System.StringComparison.OrdinalIgnoreCase))
                return colliders[i];
        }

        return null;
    }
}
