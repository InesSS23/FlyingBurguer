using UnityEngine;

public class SkyColorController : MonoBehaviour
{
    [Header("camera")]
    [SerializeField] private Camera targetCamera;

    [Header("day manager")]
    [SerializeField] private DayManager dayManager;

    [Header("cores")]
    [SerializeField] private Color normalSkyColor = new Color(0.45f, 0.75f, 1f);
    [SerializeField] private Color endingSkyColor = new Color(1f, 0.45f, 0.15f);

    [Header("tempo")]
    [SerializeField] private float warningTime = 40f;

    [Header("transição")]
    [SerializeField] private bool smoothTransition = true;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            targetCamera.backgroundColor = normalSkyColor;
        }
    }

    void Update()
    {
        if (targetCamera == null || dayManager == null)
            return;

        float timeLeft = dayManager.GetCurrentTime();

        if (timeLeft > warningTime)
        {
            targetCamera.backgroundColor = normalSkyColor;
            return;
        }

        if (smoothTransition)
        {
            float t = 1f - (timeLeft / warningTime);
            targetCamera.backgroundColor = Color.Lerp(normalSkyColor, endingSkyColor, t);
        }
        else
        {
            targetCamera.backgroundColor = endingSkyColor;
        }
    }
}