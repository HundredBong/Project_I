using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    public GameObject prefabReference { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnHit(other.gameObject);
        ObjectPoolManager.Instance.projectilePool.Return(this);
    }

    protected virtual void OnHit(GameObject other)
    {
        Debug.Log($"[Projectile] {gameObject.name} hit {other.name}");
    }
}
