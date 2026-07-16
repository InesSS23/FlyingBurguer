using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Aplica os materiais interiores apenas depois de todos os objetos da cena
/// terminarem a sua inicializacao, incluindo quando o nivel vem do MainMenu.
/// </summary>
public sealed class InteriorWallInstallRunner : MonoBehaviour
{
    private Scene targetScene;

    public void Initialize(Scene scene)
    {
        targetScene = scene;
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        const int maximumAttempts = 30;
        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            if (InteriorWallInstaller.TryInstall(targetScene))
            {
                Destroy(gameObject);
                yield break;
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }

        Debug.LogWarning("Nao foi possivel aplicar as texturas interiores da roulote.");
        Destroy(gameObject);
    }
}
