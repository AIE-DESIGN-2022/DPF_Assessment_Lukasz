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

    private UnitProducer _unitProducer;
    private EBuildState _buildState = EBuildState.Complete;
    //private Health _health;
    private MeshRenderer _meshRenderer;
    private GameController _gameController;
    private int _numberOfCollisions = 0;

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
        Complete
    }

    private new void Awake()
    {
        base.Awake();
        _unitProducer = GetComponent<UnitProducer>();
        _health = GetComponent<Health>();
        _gameController = FindObjectOfType<GameController>();
    }

    private new void Start()
    {
        base.Start();

    }

    private void Update()
    {
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
        if (_gameController.CameraController().IsInUIOffset())
        {
            transform.position = new Vector3(0.0f, 100.0f, 0.0f);
        }
        else
        {
            transform.position = _gameController.PlayerController().LocationUnderMouse();
        }

        if (_numberOfCollisions > 0)
        {
            SetBuildState(EBuildState.PlacingBad);
        }
        else
        {
            SetBuildState(EBuildState.Placing);

            if (Input.GetMouseButtonDown(0))
            {
                SetBuildState(EBuildState.Building);
                _health.NewBuilding();
                _gameController.PlayerController().PlayerControl(true);
                // based on selected units who can construct buildings, give begin actul construction order
                /*foreach (Builder builder in builders)
                {
                    builder.SetBuildTarget(this);
                }*/
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _gameController.PlayerController().PlayerControl(true);
            Destroy(gameObject);
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
                _status1 = "Producing " + _unitProducer.CurrentlyProducing().ToString() + "...";
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
                SetMaterialsColour(new Color(0, 0, 1, 0.5f));
                break;

            case EBuildState.Complete:
                GetComponent<NavMeshObstacle>().enabled = true;
                SetMaterialsColour(Color.white);
                break;
        }
    }

    private void SetMaterialsColour(Color _newColor)
    {
        if (_meshRenderer != null)
        {
            foreach (Material _material in _meshRenderer.materials)
            {
                _material.color = _newColor;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _numberOfCollisions++;
    }

    private void OnCollisionExit(Collision collision)
    {
        _numberOfCollisions--;
    }

    public EBuildState BuildState() { return _buildState; }
}
