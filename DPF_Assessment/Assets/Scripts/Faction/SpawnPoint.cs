using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GroundCoordinates()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.up * -1, out hit);
        return hit.point;
    }
}
