using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public Transform parent;
    public GameObject prefab;

    void Start()
    {
        SpawnCubes();
    }//

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(parent.GetChild(1).gameObject);
            parent.GetComponent<MeshCombiner>().Combine();
        }
    }

    void SpawnCubes()
    {
        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.transform.position = new Vector3(i * 0.2f, j * 0.2f, 0);
                obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) == transform) { continue; }

            parent.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
        }

        parent.GetComponent<MeshCombiner>().Combine();
    }
}
