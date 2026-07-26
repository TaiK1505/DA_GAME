using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    // The Dictionary! The string is the Prefab's name, and the Queue holds the recycled objects.
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    
    
    private void Awake()
    {
        // Setup the Singleton
        if (Instance == null) 
        { 
            Instance = this; 
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }
    
    public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string poolKey = prefab.name;

        // 1. If we've never shot this type of bullet before, create a new bucket for it
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        // 2. Check if we have dead bullets waiting in the bucket
        if (poolDictionary[poolKey].Count > 0)
        {
            GameObject objectToSpawn = poolDictionary[poolKey].Dequeue();
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);
            return objectToSpawn;
        }
        else
        {
            // 3. Bucket is empty We must create a brand new bullet.
            GameObject newObj = Instantiate(prefab, position, rotation);
            
           
            //rename so it exactly matches our dictionary key when returning!
            newObj.name = poolKey; 
            
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        // Turn the bullet off and throw it back into its specific bucket
        obj.SetActive(false);
        poolDictionary[obj.name].Enqueue(obj);
    }
}
