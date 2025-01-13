using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new();

    /// <summary>
    /// Empty GameObject to hold the object pool GameObjects in the Hierarchy
    /// </summary>
    private GameObject _objetPoolHolder;

    /// <summary>
    /// Empty GameObjects to hold the pooled objects (one for every different pool type)
    /// </summary>
    private static GameObject _basicBulletEmpty;
    private static GameObject _basicEnnemyEmpty;

    /// <summary>
    /// Enum to define the type of pool to use
    /// </summary>
    public enum PoolType
    {
        BasicBullet,
        BasicEnnemy,
        None
    }

    /// <summary>
    /// SetUps the empty GameObjects to hold the pooled objects on Awake
    /// </summary>
    private void Awake()
    {
        SetupEmpties();
    }

    /// <summary>
    /// Creates empty GameObjects to hold the pooled objects
    /// </summary>
    private void SetupEmpties()
    {
        _objetPoolHolder = new GameObject("PooledObjects");

        //Create a new GameObject for every PoolType
        _basicBulletEmpty = new GameObject("Basic Bullets");
        _basicBulletEmpty.transform.SetParent(_objetPoolHolder.transform);
        
        _basicEnnemyEmpty = new GameObject("Basic Ennemies");
        _basicEnnemyEmpty.transform.SetParent(_objetPoolHolder.transform);
    }

    /// <summary>
    /// Spawns an object from the pool, instantiating a new one if there is no inactive object in the pool.
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    /// <param name="poolType"></param>
    /// <returns></returns>
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
    {
        // Find pool associated with objectToSpawn
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);

        // If the pool doesn't exist, create it
        if(pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Check if there are any iActive objects in the pool
        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if(spawnableObj == null)
        {
            //If there are no inactive objects, instantiate a new one
            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            //Find the appropriate empty GameObject to hold objectToSpawn
            GameObject parentObject = GetParentObject(poolType); 
            if(parentObject != null)
            {
                spawnableObj.transform.SetParent(parentObject.transform); 
            }
        }
        else
        {
            //If there is an inactive object, reactivate it
            spawnableObj.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    /// <summary>
    /// Spawns an object from the pool, instantiating a new one if there is no inactive object in the pool.
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <param name="parentTransform"></param>
    /// <returns></returns>
    public static GameObject SpawnObject(GameObject objectToSpawn, Transform parentTransform)
    {
        // Find pool associated with objectToSpawn
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);

        // If the pool doesn't exist, create it
        if(pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Check if there are any iActive objects in the pool
        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if(spawnableObj == null)
        {
            //If there are no inactive objects, instantiate a new one
            spawnableObj = Instantiate(objectToSpawn, parentTransform);
        }
        else
        {
            //If there is an inactive object, reactivate it
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    /// <summary>
    /// Returns an object to the pool, deactivating it and adding it to the InactiveObject list
    /// </summary>
    /// <param name="gameObject"></param>
    public static void ReturnObjectToPool(GameObject gameObject)
    {
        string gameObjectName = gameObject.name.Substring(0, gameObject.name.Length - 7); //substract 7 to remove (clone) from the name of the passed gameObject

        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == gameObjectName);

        if(pool == null)
        {
            Debug.LogWarning("Trying to release an object that is not pooled: " + gameObject.name);
        }
        else
        {
            //Pool is found, add gameObject to InactiveObject list
            gameObject.SetActive(false);
            pool.InactiveObjects.Add(gameObject);
        }
    }

    /// <summary>
    /// Returns the parent GameObject for the objectToSpawn
    /// </summary>
    /// <param name="poolType"></param>
    /// <returns></returns>
    private static GameObject GetParentObject(PoolType poolType)
    {
        return poolType switch
        {
            PoolType.BasicBullet => _basicBulletEmpty,
            PoolType.BasicEnnemy => _basicEnnemyEmpty,
            PoolType.None => null,
            _ => null,
        };
    }
}

/// <summary>
/// Class to hold information about the pooled objects
/// </summary>
public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}