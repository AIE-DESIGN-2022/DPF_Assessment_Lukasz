using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    private GameObject forOfWar;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private Color32[] colors32;
    private LayerMask fogLayer;
    private Camera gameCamera;

    private void Awake()
    {
        forOfWar = gameObject;
        mesh = forOfWar.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        colors32 = new Color32[vertices.Length];
        fogLayer |= (1 << LayerMask.NameToLayer("FogOfWar"));
        
    }


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
            colors32[i] = Color.black;
        }
        gameCamera = FindObjectOfType<GameController>().CameraController().Camera();

        if (vertices.Length == 0) Debug.LogError(name + " no vertices found");
        //else print("number of vertices: " + vertices.Length);
    }

    // Update is called once per frame
    void Update()
    {
        //Test();
    }

    private void Test()
    {
        for (int i = 0; i < vertices.Length;i++)
        {
            if (i < vertices.Length/2)
            {
                colors[i].a = 0;
            }
        }
        UpdateColor();
    }

    void UpdateColor()
    {
        if (mesh != null)
        {
            mesh.colors = colors;
        }
        
    }

    void UpdateColor32()
    {
        if (mesh != null)
        {
            mesh.colors32 = colors32;
        }
    }

    public void ProcessVisibility(List<Selectable> selectables)
    {

        foreach (Selectable selectable in selectables)
        {
            
            Vector3 rayOrigin = selectable.transform.position;
            rayOrigin.y = 100;
            //Ray ray = new Ray(gameCamera.transform.position, selectable.transform.position - gameCamera.transform.position);
            Ray ray = new Ray(rayOrigin, selectable.transform.up * -1);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 300, fogLayer))
            {
                //print(selectable.name + " hit fogOfWar at " + hit.point);
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = forOfWar.transform.TransformPoint(vertices[i]);
                    float distance = Vector3.SqrMagnitude(v - hit.point);
                    float sightDistanceSqr = (selectable.SightDistance() * selectable.SightDistance());
                    if (distance < sightDistanceSqr)
                    {
                        float alpha = Mathf.Min(colors[i].a, distance / sightDistanceSqr);
                        colors[i].a = alpha;

                        if (alpha < 0.1f)
                        {
                            print(i + " set to " + alpha);
                        }
                    }
                }
                UpdateColor();
            }
        }
    }

    public void SetFogHeight(float height)
    {
        Vector3 position = transform.position;
        position.y = height;
        transform.position = position;
    }
}
