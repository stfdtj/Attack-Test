using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}
public class Attackable : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public bool destroyOnDeath = true;

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
        // TODO: drop loot, play VFX/SFX
        if (destroyOnDeath) { Destroy(gameObject); }
    }


}
