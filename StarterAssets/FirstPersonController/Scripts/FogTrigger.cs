using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class FogTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI text;

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
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 1f;
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
        StartCoroutine(ShowMessage());
    }

    private IEnumerator ShowMessage()
    {
        text.text = "C'eTaIt uN piEge, la sOrTie etait sOuS vos yeux depuis le debut et maintenant y a plus de lumiere ahahHAhaha";
        canvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        canvas.gameObject.SetActive(false);
    }
}