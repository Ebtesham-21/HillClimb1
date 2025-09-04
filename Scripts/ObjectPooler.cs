using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(this.transform); // Keep Hierarchy clean
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag) || poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist or is empty. Spawning will fail.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetParent(null); // Un-parent to move freely in the world
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Reset the state of the object's script component
        Coin coin = objectToSpawn.GetComponent<Coin>();
        if (coin != null) coin.ResetState();
        
        FuelCan fuelCan = objectToSpawn.GetComponent<FuelCan>();
        if (fuelCan != null) fuelCan.ResetState();

        DestructibleObject destructible = objectToSpawn.GetComponent<DestructibleObject>();
        if (destructible != null) destructible.ResetState();


        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist. Destroying object instead.");
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(this.transform); // Parent back to the pooler for cleanliness
        poolDictionary[tag].Enqueue(objectToReturn);
    }
    
    public void DeactivateAllPooledObjects()
    {
        foreach (var queue in poolDictionary.Values)
        {
            foreach (var obj in queue)
            {
                // This ensures we only deal with objects that were actually spawned and are still valid
                if (obj != null && obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform);
                }
            }
        }
    }
}