// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health)), RequireComponent(typeof(NavMeshObstacle))]
public class Building : Selectable
{
    [SerializeField] private EBuildingType buildingType;
    [SerializeField] private bool resourceDropPoint = false;
    [SerializeField] private float rangeOffset = 3.0f;

    private UnitProducer unitProducer;
    private EBuildState buildState = EBuildState.Complete;
    //private Health health;
    private MeshRenderer[] meshRenderers;
    private int numberOfCollisions = 0;
    private List<BuildingConstructor> constructionTeam;
    private GameObject construction;
    private GameObject rubble;
    private List<GameObject> intersectingRubble = new List<GameObject>();
    private bool farmRebuild = false;

    public enum EBuildingType
    {
        TownCenter,
        Barraks,
        University,
        Farm,
        Tower
    }

    public enum EBuildState
    {
        Placing,
        PlacingBad,
        Building,
        Complete,
        Destroyed
    }

    private new void Awake()
    {
        base.Awake();
        unitProducer = GetComponent<UnitProducer>();
        health = GetComponent<Health>();
        gameController = FindObjectOfType<GameController>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private new void Start()
    {
        base.Start();
        GetComponent<NavMeshObstacle>().carving = true;

    }

    private new void Update()
    {
        base.Update();
        switch (buildState)
        {
            case EBuildState.Placing:
                ProcessPlacement();
                break;

            case EBuildState.PlacingBad:
                ProcessPlacement();
                break;

            case EBuildState.Building:

                break;

            case EBuildState.Complete:

                break;
        }

    }

    private void ProcessPlacement()
    {
        if (gameController.CameraController().MouseIsInPlayArea())
        {
            Vector3 mouseWorldLocation = gameController.PlayerController().TerrainLocationUnderMouse();
            //transform.position = new Vector3(mouseWorldLocation.x, mouseWorldLocation.y + 1.0f, mouseWorldLocation.z);
            transform.position = mouseWorldLocation;


            if (numberOfCollisions > 0)
            {
                SetBuildState(EBuildState.PlacingBad);
            }
            else
            {
                SetBuildState(EBuildState.Placing);

                if (Input.GetMouseButtonDown(0)) // Left click to select new building location.
                {
                    SetBuildState(EBuildState.Building);
                    health.NewBuilding();
                    gameController.PlayerController().PlayerControl(true);
                    owningFaction.FinishBuildingPlacement();

                    // remove any interesting rubble
                    if (intersectingRubble.Count > 0)
                    {
                        foreach (GameObject rubble in intersectingRubble)
                        {
                            Destroy(rubble.gameObject);
                        }
                        intersectingRubble.Clear();
                    }

                    // based on selected units who can construct buildings, give begin actul construction order
                    foreach (BuildingConstructor constructor in constructionTeam)
                    {
                        constructor.SetBuildTarget(this);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1)) // Right click to cancel new building placement.
            {
                gameController.GetPlayerFaction().CancelBuildingPlacement(this);
            }
        }
        else
        {
            transform.position = new Vector3(0.0f, 100.0f, 0.0f);
        }
    }

    public EBuildingType BuildingType() { return buildingType; }

    public bool IsResourceDropPoint() { return resourceDropPoint; }

    public void Status(out string status1, out string status2)
    {
        status1 = "";
        status2 = "";

        if (unitProducer != null)
        {
            if (unitProducer.IsCurrentlyProducing())
                status1 = "Producing " + unitProducer.CurrentlyBuildingName() + "...";
        }

        if (status1 == "")
        {
            status1 = "Idle";
        }
    }

    public void ConstructBuilding(float buildRate)
    {
        health.Heal(buildRate);
        HUD_BuildingStatusUpdate();
        HUD_HealthBarUpdate();

        if (health.IsFull())
        {
            if (buildState != EBuildState.Complete) SetBuildState(EBuildState.Complete);
        }

    }

    public void SetBuildState(EBuildState newState)
    {
        if (newState == buildState) return;
        buildState = newState;

        switch (buildState)
        {
            case EBuildState.Placing:
                GetComponent<NavMeshObstacle>().enabled = false;
                SetMaterialsColour(new Color(0, 1, 0, 0.5f));
                break;

            case EBuildState.PlacingBad:
                GetComponent<NavMeshObstacle>().enabled = false;
                SetMaterialsColour(new Color(1, 0, 0, 0.5f));
                break;

            case EBuildState.Building:
                SetMinimapIndicator();
                GetComponent<NavMeshObstacle>().enabled = true;
                SetMaterialsColour(Color.white);
                AddStat();
                ConstructionSite();
                break;

            case EBuildState.Complete:
                SetMinimapIndicator();
                GetComponent<NavMeshObstacle>().enabled = true;
                ConstructionSite(false);

                break;

            case EBuildState.Destroyed:
                SetMinimapIndicator();
                GetComponent<NavMeshObstacle>().enabled = false;
                EnableRenderersAndColliders(false);
                Rubble();
                FindObjectOfType<GameController>().GetFaction(PlayerNumber()).Death(this);
                Destroy(gameObject);
                break;
        }
    }

    private void AddStat()
    {
        StatSystem statSystem = FindObjectOfType<StatSystem>();

        if (gameController.IsPlayerFaction(PlayerNumber()))
        {
            statSystem.AddStatBuilt(buildingType);
        }

    }

    private void SetMaterialsColour(Color newColor)
    {
        if (meshRenderers != null)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                foreach (Material material in meshRenderer.materials)
                {
                    material.color = newColor;
                }
            }
        }
        else Debug.LogError(name + " missing MeshRenderers.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((buildState != EBuildState.Placing && buildState != EBuildState.PlacingBad) || other.name == "Terrain") return;

        if (other.name == "Rubble")
        {
            intersectingRubble.Add(other.gameObject);
        }
        else
        {
            numberOfCollisions++;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if ((buildState != EBuildState.Placing && buildState != EBuildState.PlacingBad) || other.name == "Terrain") return;

        if (other.name == "Rubble")
        {
            intersectingRubble.Remove(other.gameObject);
        }
        else
        {
            numberOfCollisions--;
        }
        
    }

    public EBuildState BuildState() { return buildState; }

    public void SetConstructionTeam(List<BuildingConstructor> newTeam) { constructionTeam = newTeam; }

    public bool ConstructionComplete() { return buildState == EBuildState.Complete; }

    public float PercentageComplete() { return health.HealthPercentage(); }

    private void EnableRenderersAndColliders(bool enabled = true)
    {
        if (meshRenderers != null)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = enabled;
            }
        }

        MeshCollider[] colliders = GetComponentsInChildren<MeshCollider>();
        if (colliders != null)
        {
            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void ConstructionSite(bool enabled = true)
    {
        if (buildingType == EBuildingType.Farm)
        {
            CollectableResource farm = GetComponent<CollectableResource>();
            if (farm != null)
            {
                farm.ShowFarmCorn(!enabled);
            }
        }
        else
        {
            EnableRenderersAndColliders(!enabled);
            if (enabled)
            {
                GameObject constructionPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/ConstructionSite");
                if (constructionPrefab == null) Debug.LogError(name + " unable to load ConstructionSite prefab.");
                construction = Instantiate(constructionPrefab, transform.position, transform.rotation);
                construction.transform.parent = transform;
                if (buildingType != EBuildingType.Tower)
                {
                    construction.transform.localScale = Vector3.one * 1.5f;
                }
                else
                {
                    construction.transform.localScale = Vector3.one * 0.5f;
                }

            }
            else
            {
                Destroy(construction.gameObject);
                construction = null;
            }
        }
    }

    public float RangeOffset() { return rangeOffset; }

    private void Rubble(bool enabled = true)
    {
        if (enabled)
        {
            GameObject rubblePrefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rubble");
            if (rubblePrefab == null) Debug.LogError(name + " unable to load Rubble prefab.");
            rubble = Instantiate(rubblePrefab, transform.position, transform.rotation);
            rubble.transform.parent = gameController.GetMapTransform();
            rubble.name = "Rubble";
            if (buildingType != EBuildingType.Tower)
            {
                /*rubble.transform.localScale = Vector3.one * 1.5f;*/
            }
            else
            {
                rubble.transform.localScale = Vector3.one * 0.5f;
            }

        }
        else
        {
            Destroy(rubble.gameObject);
            rubble = null;
        }
    }

    public bool IsFarmRebuild()
    {
        if (buildingType == EBuildingType.Farm) return farmRebuild;
        else return false;
    }

    public void FarmRebuild(bool rebuildingFarm = true)
    {
        farmRebuild = rebuildingFarm;
    }
}
// Writen by Lukasz Dziedziczak
