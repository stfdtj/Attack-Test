using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}
public class Attackable : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    public bool destroyOnDeath = true;

    [Header("Identity")]
    public TargetType targetType;

    [Header("Loot")]
    public DropCatalog dropCatalog;

    int _hp;



    void Awake() => _hp = maxHealth;

    public void TakeDamage(int amount)
    {
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
   
        Debug.Log($"{name} took {amount}, hp={_hp}");

        if (_hp <= 0) { Die(); }
    }

    void Die()
    {
        TryDrop();
        if (destroyOnDeath) { Destroy(gameObject); }
    }

    private void TryDrop()
    {
        //Debug.Log("Trying to drop loot...");
        if (dropCatalog == null) return;

        var entry = dropCatalog.Get(targetType);
        if (entry == null || entry.prefab == null) { return; }

        if (Random.value <= entry.chance)
        {
            int count = Random.Range(entry.minCount, entry.maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                // small random offset so stacks don’t overlap perfectly
                Vector3 pos = transform.position + new Vector3(
                    Random.Range(-0.2f, 0.2f), 0.1f, Random.Range(-0.2f, 0.2f));
                Instantiate(entry.prefab, pos, Quaternion.identity);
            }
        }
    }

}
