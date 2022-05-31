// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health)), RequireComponent(typeof(NavMeshObstacle))]
public class Building : Selectable
{
    [SerializeField] private EBuildingType _buildingType;
    [SerializeField] private bool _resourceDropPoint = false;
    [SerializeField] private float rangeOffset = 3.0f;

    private UnitProducer _unitProducer;
    private EBuildState _buildState = EBuildState.Complete;
    //private Health _health;
    private MeshRenderer[] _meshRenderers;
    private int _numberOfCollisions = 0;
    private List<BuildingConstructor> _constructionTeam;
    private GameObject _construction;
    private GameObject rubble;
    private List<GameObject> intersectingRubble = new List<GameObject>();

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
        _unitProducer = GetComponent<UnitProducer>();
        _health = GetComponent<Health>();
        _gameController = FindObjectOfType<GameController>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private new void Start()
    {
        base.Start();
        GetComponent<NavMeshObstacle>().carving = true;

    }

    private new void Update()
    {
        base.Update();
        switch (_buildState)
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
        if (_gameController.CameraController().MouseIsInPlayArea())
        {
            Vector3 mouseWorldLocation = _gameController.PlayerController().TerrainLocationUnderMouse();
            //transform.position = new Vector3(mouseWorldLocation.x, mouseWorldLocation.y + 1.0f, mouseWorldLocation.z);
            transform.position = mouseWorldLocation;


            if (_numberOfCollisions > 0)
            {
                SetBuildState(EBuildState.PlacingBad);
            }
            else
            {
                SetBuildState(EBuildState.Placing);

                if (Input.GetMouseButtonDown(0)) // Left click to select new building location.
                {
                    SetBuildState(EBuildState.Building);
                    _health.NewBuilding();
                    _gameController.PlayerController().PlayerControl(true);
                    _owningFaction.FinishBuildingPlacement();

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
                    foreach (BuildingConstructor _constructor in _constructionTeam)
                    {
                        _constructor.SetBuildTarget(this);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1)) // Right click to cancel new building placement.
            {
                _gameController.GetPlayerFaction().CancelBuildingPlacement(this);
            }
        }
        else
        {
            transform.position = new Vector3(0.0f, 100.0f, 0.0f);
        }
    }

    public EBuildingType BuildingType() { return _buildingType; }

    public bool IsResourceDropPoint() { return _resourceDropPoint; }

    public void Status(out string _status1, out string _status2)
    {
        _status1 = "";
        _status2 = "";

        if (_unitProducer != null)
        {
            if (_unitProducer.IsCurrentlyProducing())
                _status1 = "Producing " + _unitProducer.CurrentlyBuildingName() + "...";
        }

        if (_status1 == "")
        {
            _status1 = "Idle";
        }
    }

    public void ConstructBuilding(float _buildRate)
    {
        _health.Heal(_buildRate);

        if (_health.IsFull())
        {
            SetBuildState(EBuildState.Complete);
        }

    }

    public void SetBuildState(EBuildState _newState)
    {
        if (_newState == _buildState) return;
        _buildState = _newState;

        switch (_buildState)
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
                GetComponent<NavMeshObstacle>().enabled = true;
                //SetMaterialsColour(new Color(0, 0, 1, 0.5f));
                EnableRenderersAndColliders(false);
                ConstructionSite();
                break;

            case EBuildState.Complete:
                GetComponent<NavMeshObstacle>().enabled = true;
                ConstructionSite(false);
                EnableRenderersAndColliders();
                SetMaterialsColour(Color.white);
                break;

            case EBuildState.Destroyed:
                GetComponent<NavMeshObstacle>().enabled = false;
                EnableRenderersAndColliders(false);
                Rubble();
                FindObjectOfType<GameController>().GetFaction(PlayerNumber()).Death(this);
                Destroy(gameObject);
                break;
        }
    }

    private void SetMaterialsColour(Color _newColor)
    {
        if (_meshRenderers != null)
        {
            foreach (MeshRenderer _meshRenderer in _meshRenderers)
            {
                foreach (Material _material in _meshRenderer.materials)
                {
                    _material.color = _newColor;
                }
            }
        }
        else Debug.LogError(name + " missing MeshRenderers.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_buildState != EBuildState.Placing && _buildState != EBuildState.PlacingBad) || other.name == "Terrain") return;

        if (other.name == "Rubble")
        {
            intersectingRubble.Add(other.gameObject);
        }
        else
        {
            _numberOfCollisions++;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if ((_buildState != EBuildState.Placing && _buildState != EBuildState.PlacingBad) || other.name == "Terrain") return;

        if (other.name == "Rubble")
        {
            intersectingRubble.Remove(other.gameObject);
        }
        else
        {
            _numberOfCollisions--;
        }
        
    }

    public EBuildState BuildState() { return _buildState; }

    public void SetConstructionTeam(List<BuildingConstructor> _newTeam) { _constructionTeam = _newTeam; }

    public bool ConstructionComplete() { return _buildState == EBuildState.Complete; }

    public float PercentageComplete() { return _health.HealthPercentage(); }

    private void EnableRenderersAndColliders(bool _enabled = true)
    {
        if (_meshRenderers != null)
        {
            foreach (MeshRenderer _meshRenderer in _meshRenderers)
            {
                _meshRenderer.enabled = _enabled;
            }
        }

        MeshCollider[] _colliders = GetComponentsInChildren<MeshCollider>();
        if (_colliders != null)
        {
            foreach (MeshCollider _collider in _colliders)
            {
                _collider.enabled = _enabled;
            }
        }
    }

    private void ConstructionSite(bool _enabled = true)
    {
        if (_enabled)
        {
            GameObject _constructionPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/ConstructionSite");
            if (_constructionPrefab == null) Debug.LogError(name + " unable to load ConstructionSite prefab.");
            _construction = Instantiate(_constructionPrefab, transform.position, transform.rotation);
            _construction.transform.parent = transform;
            if (_buildingType != EBuildingType.Tower)
            {
                _construction.transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                _construction.transform.localScale = Vector3.one * 0.5f;
            }
            
        }
        else
        {
            Destroy(_construction.gameObject);
            _construction = null;
        }
    }

    public float RangeOffset() { return rangeOffset; }

    private void Rubble(bool _enabled = true)
    {
        if (_enabled)
        {
            GameObject rubblePrefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rubble");
            if (rubblePrefab == null) Debug.LogError(name + " unable to load Rubble prefab.");
            rubble = Instantiate(rubblePrefab, transform.position, transform.rotation);
            rubble.transform.parent = _gameController.GetMapTransform();
            rubble.name = "Rubble";
            if (_buildingType != EBuildingType.Tower)
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


}
// Writen by Lukasz Dziedziczak
