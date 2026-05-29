using UnityEngine;

public class CloudSpriteMover : MonoBehaviour
{
    [Header("movimento")]
    [SerializeField] private float speed = 1f;

    [Header("limites")]
    [SerializeField] private float leftLimit = -35f;
    [SerializeField] private float rightLimit = 35f;

    [Header("camera")]
    [SerializeField] private bool faceCamera = true;

    [Header("random")]
    [SerializeField] private bool randomStartPosition = true;

    void Start()
    {
        if (randomStartPosition)
        {
            Vector3 pos = transform.position;
            pos.x = Random.Range(leftLimit, rightLimit);
            transform.position = pos;
        }
    }

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (transform.position.x > rightLimit)
        {
            Vector3 pos = transform.position;
            pos.x = leftLimit;
            transform.position = pos;
        }
    }

    void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (Camera.main == null)
            return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }
}