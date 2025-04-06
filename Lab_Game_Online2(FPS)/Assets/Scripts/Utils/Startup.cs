using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantiatePrefabs()
    {
        Debug.Log("Instantiating object");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnload");

        foreach (GameObject prefab in prefabsToInstantiate)
        {
            Debug.Log($"creating {prefab.name}");

            GameObject.Instantiate(prefab);
        }


        Debug.Log("Instantiating object done");
    }
}