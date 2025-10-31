using UnityEngine;
using System.Collections;
using System.Linq;

public class BezierSwordSlash : MonoBehaviour
{
    [Header("Refs")]
    public Transform sword;
    public Transform[] controlPoints = new Transform[5];

    [Header("Playback")]
    public float duration = 0.18f;
    public float returnTime = 0.10f;              // time to go back to idle
    public bool alignToTangent = true;
    public Vector3 upReference = Vector3.up;

    bool _slashing;
    Vector3 _idleLocalPos;
    Quaternion _idleLocalRot;

    void Awake()
    {
        _idleLocalPos = sword.localPosition;      // remember idle pose
        _idleLocalRot = sword.localRotation;
    }

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0) && !_slashing)
            StartCoroutine(SlashRoutine());
    }

    // --- conmpute Bézier curve ---
    static Vector3 BezierEvaluate(Vector3[] pts, float t)
    {
        var p = (Vector3[])pts.Clone();
        for (int k = pts.Length - 1; k > 0; k--)
            for (int i = 0; i < k; i++)
                p[i] = Vector3.LerpUnclamped(p[i], p[i + 1], t);
        return p[0];
    }
    static Vector3 BezierTangent(Vector3[] pts, float t, float eps = 1e-3f)
    {
        float t0 = Mathf.Clamp01(t - eps), t1 = Mathf.Clamp01(t + eps);
        return (BezierEvaluate(pts, t1) - BezierEvaluate(pts, t0)).normalized;
    }

    IEnumerator SlashRoutine()
    {
        _slashing = true;

        // remember loacal positions of control points
        var localPts = controlPoints.Select(p => transform.InverseTransformPoint(p.position)).ToArray();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float u = Mathf.Clamp01(t);

            Vector3 lp = BezierEvaluate(localPts, u);
            sword.localPosition = lp;

            if (alignToTangent)
            {
                Vector3 tanLocal = BezierTangent(localPts, u);
                if (tanLocal.sqrMagnitude > 1e-6f)
                    sword.localRotation = Quaternion.LookRotation(tanLocal, transform.InverseTransformDirection(upReference));
            }
            yield return null;
        }

        // return to original state
        float r = 0f;
        Vector3 startPos = sword.localPosition;
        Quaternion startRot = sword.localRotation;
        while (r < 1f)
        {
            r += Time.deltaTime / Mathf.Max(0.0001f, returnTime);
            sword.localPosition = Vector3.Lerp(startPos, _idleLocalPos, r);
            sword.localRotation = Quaternion.Slerp(startRot, _idleLocalRot, r);
            yield return null;
        }

        _slashing = false;
    }
}
