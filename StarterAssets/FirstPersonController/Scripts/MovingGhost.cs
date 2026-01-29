using UnityEngine;

public class MovingGhost : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;

    private Vector3 origin;
    private Vector3 targetPosition;
    private float targetRotationY;
    private float waitTimer;
    private bool isWaiting;

    private void Start()
    {
        origin = transform.position;
        PickNewTarget();
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                PickNewTarget();
            }
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        float currentY = transform.eulerAngles.y;
        float newY = Mathf.MoveTowardsAngle(currentY, targetRotationY, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, newY, 0f);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void PickNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        targetPosition = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);
        targetRotationY = Random.Range(0f, 360f);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? origin : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);
    }
}