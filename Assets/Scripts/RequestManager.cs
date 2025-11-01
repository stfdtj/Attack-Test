using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class AttackResponse
{
    public bool hit;
    public int damage;
    public int target_hp;
    public bool dead;
    public string encounter_id;
}

[Serializable]
public class LootItem
{
    public string item_id;
    public int qty;
}

[Serializable]
public class LootResponse
{
    public LootItem[] loot;
    public LootItem[] inventory;
}

public class RequestManager: MonoBehaviour
{
    [Header("Server address (no trailing slash)")]
    public string baseUrl = "http://localhost:8000";

    // attack
    public IEnumerator Attack(int playerId, int targetId, Action<AttackResponse> onDone)
    {
        string url = $"{baseUrl}/api/attack/";
        var bodyObj = new { player_id = playerId, target_id = targetId };
        string bodyJson = JsonUtility.ToJson(bodyObj);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Attack error: " + req.error);
            yield break;
        }

        var res = JsonUtility.FromJson<AttackResponse>(req.downloadHandler.text);
        onDone?.Invoke(res);
    }

    // loot
    public IEnumerator LootClaim(int playerId, int targetId, string encounterId, Action<LootResponse> onDone)
    {
        string url = $"{baseUrl}/api/loot/claim/";
        var bodyObj = new { player_id = playerId, target_id = targetId, encounter_id = encounterId };
        string bodyJson = JsonUtility.ToJson(bodyObj);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Loot claim error: " + req.error);
            yield break;
        }

        var res = JsonUtility.FromJson<LootResponse>(req.downloadHandler.text);
        onDone?.Invoke(res);
    }

    // inventory
    public IEnumerator GetInventory(int playerId, Action<LootItem[]> onDone)
    {
        var bodyObj = new { player_id = playerId};
        string bodyJson = JsonUtility.ToJson(bodyObj);
        string url = $"{baseUrl}/api/inventory/";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Inventory error: " + req.error);
            yield break;
        }

        var wrapper = JsonUtility.FromJson<LootResponse>(req.downloadHandler.text);
        onDone?.Invoke(wrapper.inventory);
    }
}
