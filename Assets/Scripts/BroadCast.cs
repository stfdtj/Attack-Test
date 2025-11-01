using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BroadCast : MonoBehaviour
{
    [Header("Wire this to the Content object (or leave empty to auto-find)")]
    public Transform content;
    [Min(1)] public int maxEntries = 6;
    public Font defaultFont;

    readonly Queue<GameObject> queue = new();

    void Awake()
    {
        if (content == null) content = transform.Find("Content");
        if (content == null) Debug.LogError("BroadcastFeed: missing Content child!");

        var vlg = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 4;

        var fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var crt = (RectTransform)content;
        crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
        crt.offsetMin = new Vector2(8, 8); crt.offsetMax = new Vector2(-8, -8);
    }

    public void AddMessage(string msg)
    {
        if (!content) { Debug.LogError("BroadcastFeed: Content not set"); return; }

        
        var entry = new GameObject("Entry", typeof(RectTransform), typeof(CanvasGroup));
        entry.transform.SetParent(content, false);
        var cg = entry.GetComponent<CanvasGroup>(); cg.alpha = 1f;

        var ert = (RectTransform)entry.transform;
        ert.anchorMin = new Vector2(0, 0);
        ert.anchorMax = new Vector2(1, 0);
        ert.pivot = new Vector2(0, 0);
        ert.sizeDelta = new Vector2(0, 28);

        // TMP text
        var tgo = new GameObject("Text", typeof(RectTransform));
        tgo.transform.SetParent(entry.transform, false);
        var trt = (RectTransform)tgo.transform;
        trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(6, 2); trt.offsetMax = new Vector2(-6, -2);

        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.text = msg;                       // supports rich text by default
        tmp.fontSize = 22;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        tmp.color = Color.white;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);

        // fade out
        StartCoroutine(FadeAndDestroy(cg, 4f, 0.6f));
    }

    System.Collections.IEnumerator FadeAndDestroy(CanvasGroup cg, float life, float fade)
    {
        yield return new WaitForSeconds(life);
        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - t / fade;
            yield return null;
        }
        Destroy(cg.gameObject);
    }
    public void AddLoot(string who, Item item, int amount)
    {
        string noun = (item.stackable && amount > 1) ? $"{item.displayName} ×{amount}" : item.displayName;
        AddMessage($"<b>{who}</b> obtained <color=#FFD166>{noun}</color>.");
    }
}
