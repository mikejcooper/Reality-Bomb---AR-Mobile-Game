using System;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnPool 
{
    //private GameObject _prefab;
    public NetworkHash128 AssetId { get; set; }
    public delegate GameObject SpawnDelegate(Vector3 position, NetworkHash128 assetId);
    public delegate void UnSpawnDelegate(GameObject spawned);

    private GameObject[] _pool;   

    public SpawnPool(GameObject prefab, int num)
    {
        _pool = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            _pool[i] = GameObject.Instantiate(prefab);
            _pool[i].SetActive(false);
        }

        AssetId = prefab.GetComponent<NetworkIdentity>().assetId;
        ClientScene.RegisterSpawnHandler(AssetId, SpawnObject, UnSpawnObject);
    }

    public GameObject SpawnObject(Vector3 position, NetworkHash128 assetId)
    {
        return GetObject(position);
    }

    public void UnSpawnObject(GameObject spawned)
    {
        spawned.SetActive(false);
    }

    public GameObject GetObject(Vector3 position)
    {
        foreach (GameObject go in _pool)
        {
            if (!go.activeInHierarchy)
            {
                go.transform.position = position;
                go.SetActive(true);
                return go;
            }
        }
        return null;
    }

    public int CurrentNumber()
    {
        int count = 0;
        foreach (GameObject go in _pool)
        {
            if (go.activeInHierarchy)
                count++;
        }
        return count;
    }
}

