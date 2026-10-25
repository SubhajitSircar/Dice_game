using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class Die3D : MonoBehaviour
{
    [Header("Live Speed Control (1 = Normal, 2 = Very Fast)")]
    [Range(0.5f, 3.0f)]
    public float spinSpeedMultiplier = 1.5f;

    private Rigidbody rb;
    private Vector3 startPosition;

    public int CurrentValue { get; private set; }
    public bool IsRolling { get; private set; }

    private readonly (Vector3 localDir, Vector3 eulerRot, int value)[] faces = new[]
    {
        (Vector3.up,      new Vector3(90f, 0f, 0f),   1),
        (Vector3.down,    new Vector3(-90f, 0f, 0f),  6),
        (Vector3.forward, new Vector3(0f, 180f, 0f),  2),
        (Vector3.back,    new Vector3(0f, 0f, 0f),    5),
        (Vector3.right,   new Vector3(0f, -90f, 0f),  3),
        (Vector3.left,    new Vector3(0f, 90f, 0f),   4)
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 100f; 
        startPosition = transform.position;
        CreateFaceNumbers();
    }

    private void CreateFaceNumbers()
    {
        foreach (var face in faces)
        {
            GameObject labelObj = new GameObject($"Face_{face.value}");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = face.localDir * 0.51f;
            labelObj.transform.localRotation = Quaternion.Euler(face.eulerRot);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = face.value.ToString();
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1f, 1f);
        }
    }

    public void Roll(float totalDuration, int forcedValue = -1)
    {
        StopAllCoroutines();
        StartCoroutine(RollRoutine(totalDuration, forcedValue));
    }

    private IEnumerator RollRoutine(float totalDuration, int forcedValue)
    {
        IsRolling = true;

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.drag = 0.1f;
        rb.angularDrag = 0.05f; 

        
        transform.position = startPosition + new Vector3(
            Random.Range(-0.2f, 0.2f),
            0.6f,
            Random.Range(-0.2f, 0.2f)
        );
        transform.rotation = Random.rotation;

        
        Vector3 toCenter = (new Vector3(0f, 0.5f, 0f) - transform.position).normalized;
        rb.velocity = toCenter * Random.Range(2.5f, 4.5f) + new Vector3(
            Random.Range(-1.5f, 1.5f),
            -3.5f,
            Random.Range(-1.5f, 1.5f)
        );

        
        Vector3 spinAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        float targetSpinSpeed = Random.Range(22f, 34f) * spinSpeedMultiplier;
        rb.angularVelocity = spinAxis * targetSpinSpeed;

        float physicsTime = totalDuration * 0.72f;
        float settleTime = totalDuration * 0.28f;

        
        float t = 0f;
        while (t < physicsTime)
        {
            t += Time.fixedDeltaTime;
            float progress = t / physicsTime;

            
            float currentSpeed = Mathf.Lerp(targetSpinSpeed, targetSpinSpeed * 0.35f, progress);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, spinAxis * currentSpeed, 0.25f);

            rb.AddForce(Vector3.down * 12f, ForceMode.Acceleration);
            yield return new WaitForFixedUpdate();
        }

        
        rb.isKinematic = true;
        Quaternion startRot = transform.rotation;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, 0.5f, startPos.z);

        int targetFace = (forcedValue != -1) ? forcedValue : GetTopFaceByDotProduct();
        Quaternion targetRot = GetTargetRotationForFace(targetFace);

        float elapsed = 0f;
        while (elapsed < settleTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / settleTime);
            float ease = 1f - Mathf.Pow(1f - progress, 3f);

            transform.rotation = Quaternion.Slerp(startRot, targetRot, ease);
            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        transform.rotation = targetRot;
        transform.position = targetPos;

        CurrentValue = GetTopFaceByDotProduct();
        IsRolling = false;
    }

    public int GetTopFaceByDotProduct()
    {
        float maxDot = -Mathf.Infinity;
        int bestFace = 1;

        foreach (var face in faces)
        {
            Vector3 worldDir = transform.TransformDirection(face.localDir);
            float dot = Vector3.Dot(worldDir, Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestFace = face.value;
            }
        }
        return bestFace;
    }

    private Quaternion GetTargetRotationForFace(int faceValue)
    {
        Vector3 localDir = Vector3.up;
        foreach (var face in faces)
        {
            if (face.value == faceValue)
            {
                localDir = face.localDir;
                break;
            }
        }

        Vector3 currentUpWorld = transform.TransformDirection(localDir);
        Quaternion tipToFlat = Quaternion.FromToRotation(currentUpWorld, Vector3.up) * transform.rotation;
        Vector3 euler = tipToFlat.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        return Quaternion.Euler(euler);
    }
}