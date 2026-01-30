using UnityEngine;

public class MatchStick : MonoBehaviour
{
    [SerializeField] private float duration = 60f;
    [SerializeField] private float startDensity = 0.95f;
    [SerializeField] private float endDensity = 0.4f;
    [SerializeField] private Vector3 attachOffset = new Vector3(0.5f, 0f, 0.5f);
    [SerializeField] private Color emissionColor = Color.yellow;
    [SerializeField] private float maxEmissionIntensity = 2f;

    private static MatchStick active;

    private float timer;
    private Material material;

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<CharacterController>();
        if (controller == null) return;

        float remainingTime = 0f;

        if (active != null)
        {
            remainingTime = active.timer;
            Destroy(active.gameObject);
        }

        active = this;
        timer = duration + remainingTime;

        transform.SetParent(controller.transform);
        transform.localPosition = attachOffset;

        RenderSettings.fogDensity = endDensity;

        material = GetComponentInChildren<Renderer>().material;
        material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        if (active != this) return;

        timer -= Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        material.SetColor("_EmissionColor", emissionColor * maxEmissionIntensity * t);

        if (timer <= 0f)
        {
            RenderSettings.fogDensity = startDensity;
            active = null;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (active == this)
            active = null;
    }
}