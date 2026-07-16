using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Melhora apenas a apresentacao do HUD existente. Nao altera anchors,
/// tamanhos, layout groups nem os dados controlados pelos restantes scripts.
/// </summary>
public sealed class SafeHUDVisualPolish : MonoBehaviour
{
    private static readonly Color Cocoa = new Color(0.24f, 0.105f, 0.055f, 1f);
    private static readonly Color Coral = new Color(0.95f, 0.29f, 0.18f, 1f);
    private static readonly Color Peach = new Color(1f, 0.66f, 0.28f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith("Level"))
            return;

        if (FindFirstObjectByType<SafeHUDVisualPolish>() == null)
            new GameObject("Safe HUD Visual Polish").AddComponent<SafeHUDVisualPolish>();
    }

    private IEnumerator Start()
    {
        yield return null;
        PolishDayPanel();
        PolishOrdersPanel();
    }

    private void PolishDayPanel()
    {
        GameObject panel = GameObject.Find("DayHUDPanel");
        if (panel == null)
            return;

        AddSoftShadow(panel, new Vector2(5f, -5f));
        TintNamedImage(panel.transform, "FundoDia", Coral);
        TintNamedImage(panel.transform, "FundoTimer", Peach);
        TintNamedImage(panel.transform, "FundoScore", new Color(0.83f, 0.95f, 0.72f, 1f));

        StyleNamedText(panel.transform, "DayText", Color.white, 0.16f);
        StyleNamedText(panel.transform, "TimerText", Cocoa, 0.08f);
        StyleNamedText(panel.transform, "ScoreText", Cocoa, 0.08f);
    }

    private void PolishOrdersPanel()
    {
        // desativado: o quadro de pedidos passou a ser um World Space Canvas na parede,
        // com o seu proprio fundo (Quadro_Pedidos) e tamanhos afinados manualmente -
        // o cabecalho/sublinhado e o forcar de escala desta classe eram para o HUD antigo
        // do canto do ecra e estavam a conflituar com esse novo layout
    }

    private static void TintNamedImage(Transform root, string objectName, Color color)
    {
        Transform child = root.Find(objectName);
        if (child == null)
            return;

        Image image = child.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }

    private static void StyleNamedText(Transform root, string objectName, Color color, float outlineWidth)
    {
        Transform child = root.Find(objectName);
        if (child == null)
            return;

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text == null)
            return;

        text.color = color;
        text.fontStyle = FontStyles.Bold;
        text.outlineColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.5f);
        text.outlineWidth = outlineWidth;
    }

    private static void AddSoftShadow(GameObject target, Vector2 distance)
    {
        if (target.GetComponent<Shadow>() != null)
            return;

        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.28f);
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }
}
