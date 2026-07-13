using UnityEngine;

[System.Serializable]
public class DialogueFrame
{
    [TextArea(2, 5)]
    public string text;

    public Sprite image;
}

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Flying Burger/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Identificacao do nivel")]
    public int dayNumber = 1;
    public string sceneName = "Level1";
    public string nextSceneName = "";
    public bool isFinalLevel = false;
    public string finalSceneName = "";

    [Header("Objetivo do dia")]
    public float dayDuration = 300f;
    public int targetScore = 100;
    public int pointsPerDelivery = 10;

    [Header("Clientes")]
    public float minSpawnTime = 8f;
    public float maxSpawnTime = 15f;
    public float customerPatienceTime = 25f;
    public int missedCustomerPenalty = 5;

    [Header("Ingredientes permitidos neste nivel")]
    public bool allowCheese = true;
    public bool allowLettuce = true;
    public bool allowTomato = true;
    public bool allowPepper = true;

    [Header("Extras permitidos neste nivel")]
    public bool allowFries = true;
    public bool allowDrink = true;

    [Header("Complexidade dos hamburgueres")]
    [Tooltip("0 = so pao + carne + pao. 1 = um ingrediente extra. 2 = ate dois ingredientes extra. 3 = ate tres ingredientes extra.")]
    public int maxExtraIngredients = 3;

    [Header("Dialogo / cutscene inicial")]
    public DialogueFrame[] introFrames;

    [Header("Cutscene final")]
    public DialogueFrame[] endFrames;

    [Header("Audio")]
    public AudioClip gameplayBackgroundMusic;
}