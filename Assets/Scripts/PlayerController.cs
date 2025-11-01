using System;
using System.Collections;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Transform body;

    [Header("Sensitivity")]
    public float sensX = 0.15f;
    public float sensY = 0.15f;
    public bool invertY = false;

    [Header("Pitch clamp (vertical look)")]
    public float pitchMin = -85f;  // down
    public float pitchMax = 85f;  // up

    float yawWorld;   // absolute world yaw we set on body
    float pitchLocal;

    [Header("Speed")]
    public float moveSpeed = 5.0f;

    [Header("Camera")]
    public Camera playerCamera;

    [Header("Server")]
    public int playerId = 1;
    public string baseUrl = "http://localhost:8000";

    public int damage = 25;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // initialize from current transforms
        yawWorld = body.rotation.eulerAngles.y;
  

        // keep physics from fighting rotation
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

 
        yawWorld += d.x * sensX;


        float dy = (invertY ? d.y : -d.y) * sensY;
        pitchLocal = Mathf.Clamp(pitchLocal + dy, pitchMin, pitchMax);

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
            var target = aimer.CurrentTarget.GetComponentInParent<Attackable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                
                StartCoroutine(SendAttack(playerId, target.id));
            }
        }
    }

    Vector2 ReadMouseDelta()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.delta.ReadValue();
        #endif

    
        float mx = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 1000f; // scale to feel similar
        float my = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 1000f;
        return new Vector2(mx, my);
    }
    [Serializable] class AttackReq { public int player_id; public int target_id; }
    [Serializable] class LootReq { public int player_id; public int target_id; public string encounter_id; }
    [Serializable] class AttackRes { public bool hit; public int damage; public int target_hp; public bool dead; public string encounter_id; }


    IEnumerator SendAttack(int playerId, int targetId)
    {
        string url = $"{baseUrl}/api/attack/";
        var bodyObj = new AttackReq { player_id = playerId, target_id = targetId };
        string json = JsonUtility.ToJson(bodyObj);

        using var req = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("X-Idempotency-Key", System.Guid.NewGuid().ToString());

        Debug.Log($"POST {url} body={json}");
        yield return req.SendWebRequest();

        Debug.Log($"{req.responseCode} {req.error} {req.downloadHandler.text}");
        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<AttackRes>(req.downloadHandler.text);
            if (res.dead) StartCoroutine(SendLootClaim(playerId, targetId, res.encounter_id));
        }
    }

    IEnumerator SendLootClaim(int playerId, int targetId, string encounterId)
    {
        string url = $"{baseUrl}/api/loot/claim/";
        var bodyObj = new LootReq { player_id = playerId, target_id = targetId, encounter_id = encounterId };
        string json = JsonUtility.ToJson(bodyObj);

        using var req = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"POST {url} body={json}");
        yield return req.SendWebRequest();

        Debug.Log($"{req.responseCode} {req.error} {req.downloadHandler.text}");
    }

    [Serializable]
    public class AttackResult
    {
        public bool hit;
        public int damage;
        public int target_hp;
        public bool dead;
        public string encounter_id;
    }


}
