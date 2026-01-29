using UnityEngine;

public class MatchStick : MonoBehaviour
{
    [SerializeField] private float duration = 60f;
    [SerializeField] private float startDensity = 0.95f;
    [SerializeField] private float endDensity = 0.4f;
    [SerializeField] private Vector3 attachOffset = new Vector3(0.5f, 0f, 0.5f);

    private Transform player;
    private float timer;
    private bool isActive;

    private void OnTriggerEnter(Collider other)
    {
        if (isActive) return;

        var controller = other.GetComponent<CharacterController>();
        if (controller == null) return;

        player = controller.transform;
        transform.SetParent(player);
        transform.localPosition = attachOffset;

        RenderSettings.fogDensity = endDensity;
        isActive = true;
        timer = duration;
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            RenderSettings.fogDensity = startDensity;
            Destroy(gameObject);
        }
    }
}