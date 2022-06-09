using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Tree/Tree Corrections")]
public class TreeCorrestion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void MoveToGroundLevel()
    {
        CollectableResource[] trees = FindObjectsOfType<CollectableResource>();

        foreach (CollectableResource tree in trees)
        {
            if (tree.ResourceType() != CollectableResource.EResourceType.Wood) return;

            LayerMask terrainMask2 = new LayerMask();
            terrainMask2 |= (1 << LayerMask.NameToLayer("Terrain"));

            Vector3 rayOrigin = transform.position;
            rayOrigin.y += 50;

            float terrainLevel = 0;

            Ray ray = new Ray(rayOrigin, transform.up * -1);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 500, terrainMask2))
            {
                terrainLevel = hit.point.y;
            }
            else
            {
                Debug.Log("no hit");
            }

            if (terrainLevel != 0)
            {
                transform.position = new Vector3(rayOrigin.x, terrainLevel, rayOrigin.z);
            }
        }

        

    }
}
