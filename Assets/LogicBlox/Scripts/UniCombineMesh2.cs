using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [RequireComponent(typeof(MeshFilter))]
// [RequireComponent(typeof(MeshRenderer))]

public class UniCombineMesh2 : MonoBehaviour
{

    public GameObject[] sources = new GameObject[2];

    List<CombineInstance> combines = new List<CombineInstance>();

    // Use this for initialization
    void Start()
    {
        if ((sources[0] == null || sources[1] == null) || (sources[0] == sources[1]))
        {
           // Debug.LogError("no source or same sources");
            return;
        }

        Matrix4x4 source_matrix = sources[0].transform.worldToLocalMatrix;

        for (int i = 0; i < sources.Length; i++)
        {
            MeshFilter mesh_filter = sources[i].GetComponent<MeshFilter>();
            if (mesh_filter == null)
            {
                Debug.LogError(sources[i] + "no meshfilter");
                return;
            }

            CombineInstance combine_instance = new CombineInstance
            {
                mesh = mesh_filter.sharedMesh,
                transform = source_matrix * mesh_filter.transform.localToWorldMatrix
            };
            combines.Add(combine_instance);
        }

        MeshFilter combined_filter = sources[0].GetComponent<MeshFilter>();
        combined_filter.sharedMesh = null;
        combined_filter.sharedMesh = new Mesh();

        combined_filter.sharedMesh.CombineMeshes(combines.ToArray(), false, false);

        //Debug.Log("meshes combined2");
    }


}
  
