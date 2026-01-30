using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private SplineChaser splineChaser;
    private bool triggered;

    private void Start()
    {
        canvas.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || other.GetComponent<CharacterController>() == null)
            return;

        triggered = true;
        StartCoroutine(EventCoroutine());
    }

    private IEnumerator EventCoroutine()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.2f;
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
        text.text = "Dégage !!!  DEGAGE SORT DE LA";
        canvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(4);
        splineChaser.Play();
        yield return new WaitForSeconds(4);
        RenderSettings.fogDensity = 0.95f;
        yield return new WaitForSeconds(2f);
        canvas.gameObject.SetActive(false);
    }
}