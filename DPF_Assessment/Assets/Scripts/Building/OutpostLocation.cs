using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostLocation : MonoBehaviour
{
    private bool occupied = false;

    public Vector3 position { get { return transform.position; } }

    public bool isOccupied { get { return occupied; } }

    public void Occupy() 
    {
        occupied = true; 
    }
}
