using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public bool doubleChild = false;

    public void Combine(bool doubleChild = false)
    {
        this.doubleChild = doubleChild;

        HideOldMesh();
        CombineMeshes();
    }

    private void HideOldMesh()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child == transform) { continue; }

            if (doubleChild)
            {
                for (int j = 0; j < child.childCount; j++)
                {
                    if (child.GetChild(j) == child) { continue; }

                    child.GetChild(j).GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void CombineMeshes()
    {
        Quaternion oldRot = transform.rotation;
        Vector3 oldPos = transform.position;

        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();

        Mesh finalMesh = new Mesh();

        CombineInstance[] combiners = new CombineInstance[filters.Length - 1];

        for (int i = 1; i < filters.Length; i++) // skip index 0, i.e itself.
        {
            //if (filters[i].transform == transform) { continue; } // itself

            combiners[i - 1].subMeshIndex = 0;
            combiners[i - 1].mesh = filters[i].sharedMesh;
            combiners[i - 1].transform = filters[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combiners);

        GetComponent<MeshFilter>().sharedMesh = finalMesh;

        transform.rotation = oldRot;
        transform.position = oldPos;
    }

}
