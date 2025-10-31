using System;
using UnityEngine;

public class ClampedLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform body;     // player root with rigidbody

    [Header("Sensitivity")]
    // For legacy Input: units are deg per mouse unit per second.
    // For new Input System: units are deg per pixel.
    public float sensX = 0.15f;
    public float sensY = 0.15f;
    public bool invertY = false;

    [Header("Pitch clamp (vertical look)")]
    public float pitchMin = -85f;  // down
    public float pitchMax = 85f;  // up

    float yawWorld;   // absolute world yaw we set on body
    float pitchLocal;

    [Header("Speed")]
    public float moveSpeed = 5.0f;          // walk speed (m/s)

    [Header("Camera")]
    public Camera playerCamera;

    public int damage = 25;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize from current transforms
        yawWorld = body.rotation.eulerAngles.y;
  

        // Keep physics from fighting rotation
        var rb = body.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Update()
    {
        // look around
        Vector2 d = ReadMouseDelta();

        // Yaw = free 360 (no clamp)
        yawWorld += d.x * sensX;

        // Pitch = clamped
        float dy = (invertY ? d.y : -d.y) * sensY;
        pitchLocal = Mathf.Clamp(pitchLocal + dy, pitchMin, pitchMax);

        // Apply
        body.rotation = Quaternion.Euler(0f, yawWorld, 0f);
   
        // move
        if (Input.GetKey(KeyCode.W))
        {
            body.position += body.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            body.position -= body.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            body.position -= body.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            body.position += body.right * moveSpeed * Time.deltaTime;
        }
        // targeting
        var aimer = playerCamera.GetComponent<CenterDotTarget>();
        if (Input.GetMouseButtonDown(0) && aimer && aimer.CurrentTarget)
        {
            var dmg =aimer.CurrentTarget.GetComponentInParent<IDamageable>();
            if (dmg != null) { dmg.TakeDamage(damage); }
            //Debug.Log("Hit: " + aimer.CurrentTarget.name);
        }
    }

    Vector2 ReadMouseDelta()
    {
        // New Input System first (pixel delta; do NOT multiply by Time.deltaTime)
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.delta.ReadValue();
        #endif

        // Legacy Input Manager fallback (frame-scaled axes; DO multiply by dt)
        float mx = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 1000f; // scale to feel similar
        float my = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 1000f;
        return new Vector2(mx, my);
    }


}
