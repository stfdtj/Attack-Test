using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterDotTarget : MonoBehaviour
{
    [Header("UI")]
    public float dotSize = 6f;
    public Color dotColor = Color.red;

    [Header("Targeting")]
    public float maxDistance = 50f;
    public LayerMask targetMask = ~0; // everything by default

    public Collider CurrentTarget { get; private set; } // read-only outside

    Texture2D _dot;

    void Awake()
    {
        _dot = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _dot.SetPixel(0, 0, dotColor);
        _dot.Apply();
    }

    void Update()
    {
        // Ray from center of screen (viewport 0..1)
        Ray ray = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetMask, QueryTriggerInteraction.Ignore))
        {
            CurrentTarget = hit.collider;

        }
        else
        {
            CurrentTarget = null;
        }
    }

    void OnGUI()
    {
        float x = (Screen.width - dotSize) * 0.5f;
        float y = (Screen.height - dotSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), _dot);
    }
}