using UnityEngine;

public class FloatingUIElement : MonoBehaviour
{
    [Header("Float Movement")]
    public float verticalAmplitude = 12f;
    public float horizontalAmplitude = 8f;
    public float speed = 1f;

    [Header("Rotation")]
    public float rotationAmplitude = 2f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float startRotationZ;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        startRotationZ = rectTransform.localEulerAngles.z;
    }

    private void Update()
    {
        float time = Time.unscaledTime * speed;

        float offsetX = Mathf.Sin(time * 0.8f) * horizontalAmplitude;
        float offsetY = Mathf.Sin(time) * verticalAmplitude;
        float rotationZ = Mathf.Sin(time * 0.7f) * rotationAmplitude;

        rectTransform.anchoredPosition = startPosition + new Vector2(offsetX, offsetY);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, startRotationZ + rotationZ);
    }
}