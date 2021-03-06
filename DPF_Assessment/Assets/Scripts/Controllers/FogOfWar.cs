using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    private GameObject fogOfWar;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private Color32[] colors32;
    private Vector3[] positions;
    private LayerMask fogLayer;
    private Camera gameCamera;
    private List<int> changed;
    private CancellationTokenSource tokenSource;
    private float taskTimer = 1;
    private bool timerStarted = false;
    private float lastTaskDuration = Mathf.Infinity;
    [SerializeField] private float sightDistanceOffSet = 1;

    private void Awake()
    {
        fogOfWar = gameObject;
        mesh = fogOfWar.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        colors32 = new Color32[vertices.Length];
        positions = new Vector3[vertices.Length];
        fogLayer = new LayerMask();
        fogLayer |= (1 << LayerMask.NameToLayer("FogOfWar"));
        //fogLayer = fogOfWar.layer;
        changed = new List<int>();
    }


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Color.black;
            colors32[i] = Color.black;
            positions[i] = fogOfWar.transform.TransformPoint(vertices[i]);
        }
        gameCamera = FindObjectOfType<GameController>().CameraController().Camera();

        if (vertices.Length == 0) Debug.LogError(name + " no vertices found");

        tokenSource = new CancellationTokenSource();
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStarted) taskTimer += Time.deltaTime;
        else if (taskTimer > 0)
        {
            lastTaskDuration = taskTimer;
            taskTimer = 0;
        }
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
        TurnBackAllChanged();
        List<Vector3> fogHits = new List<Vector3>();

        foreach (Selectable selectable in selectables)
        {
            Vector3 rayOrigin = selectable.transform.position;
            rayOrigin.y += 50;
            Vector3 rayDirection = selectable.transform.up * -1;
            Ray ray = new Ray(rayOrigin, rayDirection);
            RaycastHit hit;

            
            if (Physics.Raycast(ray, out hit, 100, fogLayer))
            {
                fogHits.Add(hit.point);
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = 0;
            float sightDistanceSqr = 20*20;

            foreach (Vector3 fogHit in fogHits)
            {
                distance = Vector3.SqrMagnitude(positions[i] - fogHit);

                if (distance != 0 && distance < sightDistanceSqr)
                {
                    //float alpha = Mathf.Min(colors[i].a, distance / sightDistanceSqr);
                    //colors[i].a = alpha;
                    colors[i].a = 0;
                    changed.Add(i);
                }
            }

            
        }
        UpdateColor();
    }

    public void SetFogHeight(float height)
    {
        Vector3 position = transform.position;
        position.y = height;
        transform.position = position;
    }

    private void TurnBackAllChanged()
    {
        foreach (int vert in changed)
        {
            colors[vert].a = 0.2f;
        }
        changed.Clear();
    }

    public async void ProcessVisibilityAsync(List<Selectable> selectables)
    {
        //print("async task start");
        timerStarted = true;
        bool complete = false;

        List<Vector3> fogHits = new List<Vector3>();
        List<float> sightDistances = new List<float>();
        foreach (Selectable selectable in selectables)
        {
            Vector3 rayOrigin = selectable.transform.position;
            rayOrigin.y += 50;
            Vector3 rayDirection = selectable.transform.up * -1;
            Ray ray = new Ray(rayOrigin, rayDirection);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 100, fogLayer))
            {
                fogHits.Add(hit.point);
            }

            sightDistances.Add(selectable.SightDistance() + sightDistanceOffSet);
        }

        complete = await Task.Run(() => 
        {
            foreach (int vert in changed)
            {
                colors[vert].a = 0.2f;

                if (tokenSource.IsCancellationRequested)
                {
                    return false;
                }
            }
            changed.Clear();

            for (int i = 0; i < vertices.Length; i++)
            {
                float distanceSqr = 0;
                

                for (int s = 0; s < fogHits.Count; s++)
                {
                    distanceSqr = Vector3.SqrMagnitude(positions[i] - fogHits[s]);
                    float sightDistanceSqr = sightDistances[s] * sightDistances[s];

                    if (distanceSqr != 0 && distanceSqr < sightDistanceSqr)
                    {
                        //float alpha = Mathf.Min(colors[i].a, distanceSqr / sightDistanceSqr);
                        //colors[i].a = alpha;
                        colors[i].a = 0;
                        changed.Add(i);
                    }

                    if (tokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                }

                /*foreach (Vector3 fogHit in fogHits)
                {
                    distanceSqr = Vector3.SqrMagnitude(positions[i] - fogHit);

                    if (distanceSqr != 0 && distanceSqr < sightDistanceSqr)
                    {
                        //float alpha = Mathf.Min(colors[i].a, distance / sightDistanceSqr);
                        //colors[i].a = alpha;
                        colors[i].a = 0;
                        changed.Add(i);
                    }

                    if (tokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                }*/


            }
            return true;
        },tokenSource.Token);

        if(tokenSource.IsCancellationRequested)
        {
            return;
        }

        if (mesh != null) mesh.colors = colors;

        //print("async task complete");
        timerStarted = false;
    }

    public float LastTaskDuration { get { return lastTaskDuration; } }

    public bool IsReady { get { return !timerStarted; } }
}
