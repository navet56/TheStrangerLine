using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MobCollider : MonoBehaviour
{
    [SerializeField] private float displayDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterController>(out var controller))
        {
            StartCoroutine(HandleGameOver(controller));
        }
    }

    private IEnumerator HandleGameOver(CharacterController controller)
    {
        controller.enabled = false;
        controller.transform.position = Floor.Instance.Spawn.position;
        controller.enabled = true;

        if (Floor.Instance.DisplayText != null)
        {
            Floor.Instance.DisplayText.text = "Game Over";
            Floor.Instance.DisplayText.gameObject.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            Floor.Instance.DisplayText.gameObject.SetActive(false);
        }
    }
}