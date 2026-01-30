using UnityEngine;
using UnityEngine.Splines;

public class SplineChaser : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] SplineContainer spline;

    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] bool loop = true;
    [SerializeField] float rotationSpeed = 10f;

    float progress;
    float splineLength;
    bool isRunning;
    bool finished;

    void Start()
    {
        splineLength = spline.CalculateLength();
    }

    void Update()
    {
        if (!isRunning || finished) return;

        progress += speed * Time.deltaTime / splineLength;

        if (loop)
        {
            progress = Mathf.Repeat(progress, 1f);
        }
        else if (progress >= 1f)
        {
            progress = 1f;
            finished = true;
            isRunning = false;
        }

        transform.position = spline.EvaluatePosition(progress);

        Vector3 tangent = spline.EvaluateTangent(progress);
        if (tangent != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(tangent);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Play() => isRunning = true;

    public void Pause() => isRunning = false;

    public void Reset()
    {
        progress = 0f;
        finished = false;
        isRunning = false;
        transform.position = spline.EvaluatePosition(0f);
        transform.rotation = Quaternion.LookRotation(spline.EvaluateTangent(0f));
    }

    public void Restart()
    {
        Reset();
        Play();
    }

    public float GetProgress() => progress;
    public bool IsRunning() => isRunning;
    public bool IsFinished() => finished;
}