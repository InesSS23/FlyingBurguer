using UnityEngine;

public class CloudMover : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 25f;
    public bool moveRight = true;

    [Header("Loop Settings")]
    public float leftLimit = -1200f;
    public float rightLimit = 1200f;

    [Header("Random Height")]
    public bool randomizeYOnLoop = true;
    public float minY = -250f;
    public float maxY = 350f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        float direction = moveRight ? 1f : -1f;

        rectTransform.anchoredPosition += Vector2.right * direction * speed * Time.unscaledDeltaTime;

        if (moveRight && rectTransform.anchoredPosition.x > rightLimit)
        {
            ResetToLeft();
        }
        else if (!moveRight && rectTransform.anchoredPosition.x < leftLimit)
        {
            ResetToRight();
        }
    }

    private void ResetToLeft()
    {
        float newY = randomizeYOnLoop ? Random.Range(minY, maxY) : rectTransform.anchoredPosition.y;
        rectTransform.anchoredPosition = new Vector2(leftLimit, newY);
    }

    private void ResetToRight()
    {
        float newY = randomizeYOnLoop ? Random.Range(minY, maxY) : rectTransform.anchoredPosition.y;
        rectTransform.anchoredPosition = new Vector2(rightLimit, newY);
    }
}