using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ObjectPool<BulletPool, BulletObject, Vector2>
{
    static protected Dictionary<GameObject, BulletPool> poolInstances = new Dictionary<GameObject, BulletPool>();

    private void Awake()
    {
        if (prefab != null && !poolInstances.ContainsKey(prefab))
        {
            poolInstances.Add(prefab, this);
        }
    }

    private void OnDestroy()
    {
        poolInstances.Remove(prefab);
    }

    static public BulletPool GetObjectPool(GameObject prefab, int initialPoolCount = 10)
    {
        BulletPool objPool = null;
        if (!poolInstances.TryGetValue(prefab, out objPool))
        {
            GameObject obj = new GameObject(prefab.name + "_Pool");
            objPool = obj.AddComponent<BulletPool>();
            objPool.prefab = prefab;
            objPool.initialPoolCount = initialPoolCount;

            poolInstances[prefab] = objPool;
        }

        return objPool;
    }
}

public class BulletObject : PoolObject<BulletPool, BulletObject, Vector2>
{
    public Transform transform;
    public Rigidbody2D rigidbody2D;
    public SpriteRenderer spriteRenderer;
    public Bullet bullet;

    protected override void SetReferences()
    {
        transform = instance.transform;
        rigidbody2D = instance.GetComponent<Rigidbody2D>();
        spriteRenderer = instance.GetComponent<SpriteRenderer>();
        bullet = instance.GetComponent<Bullet>();
        bullet.bulletPoolObject = this;
        bullet.mainCamera = Object.FindObjectOfType<Camera>();
    }

    public override void WakeUp(Vector2 position)
    {
        transform.position = position;
        instance.SetActive(true);
    }

    public override void Sleep()
    {
        instance.SetActive(false);
    }
}