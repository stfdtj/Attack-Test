using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Loot/Drop Catalog")]
public class DropCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public TargetType type;
        public GameObject prefab;          // what to drop
        [Range(0f, 1f)] public float chance = 1f; // 1 = always drop
        public int minCount = 1;
        public int maxCount = 1;
    }

    [SerializeField] private List<Entry> entries = new();

    // lookup an entry by TargetType
    public Entry Get(TargetType type)
    {
        return entries.Find(e => e.type == type);
    }
}
