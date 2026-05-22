using System.Collections;
using TMPro;
using UnityEngine;

public class CustomerSpeechUI : MonoBehaviour
{
    [Header("ui da fala")]
    [SerializeField] private GameObject speechPanel;
    [SerializeField] private TMP_Text speechText;

    [Header("tempo visivel")]
    [SerializeField] private float speechDuration = 2.5f;

    [Header("camera")]
    [SerializeField] private bool faceCamera = true;

    private Coroutine speechCoroutine;

    void Start()
    {
        HideSpeech();
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

    public void ShowSpeech(string message)
    {
        Debug.Log("vou mostrar fala: " + message);

        if (speechPanel == null)
        {
            Debug.Log("SpeechPanel nao esta ligado");
            return;
        }

        if (speechText == null)
        {
            Debug.Log("SpeechText nao esta ligado");
            return;
        }

        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
        }

        speechCoroutine = StartCoroutine(ShowSpeechRoutine(message));
    }

    private IEnumerator ShowSpeechRoutine(string message)
    {
        speechText.text = message;
        speechPanel.SetActive(true);

        yield return new WaitForSeconds(speechDuration);

        HideSpeech();
    }

    public void HideSpeech()
    {
        if (speechPanel != null)
        {
            speechPanel.SetActive(false);
        }
    }
}