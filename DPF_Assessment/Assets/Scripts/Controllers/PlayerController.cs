// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private List<Selectable> _currentSelection;
    private int _playerNumber = 1;
    private HUD_Manager _hudManager;
    private GameCameraController _cameraController;
    private bool _playerControlOnline = true;

    private void Awake()
    {
        _cameraController = FindObjectOfType<GameCameraController>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        _currentSelection = new List<Selectable>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_cameraController.IsInUIOffset()) return;
        if (!_playerControlOnline) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();

    }

    private void HandleLeftClick()
    {
        ClearSelection();

        RaycastHit _hit = UnderMouse();
        Selectable _selectable = _hit.transform.GetComponent<Selectable>();
        if (_selectable != null && (_selectable.PlayerNumber() == _playerNumber || _selectable.PlayerNumber() == 0))
        {
            //Debug.Log("Selectable");
            _selectable.Selected(true);
            _currentSelection.Add(_selectable);
            _hudManager.NewSelection(_currentSelection);
        }
        //Debug.Log("Not Selectable " + _hit.transform.gameObject.name);
    }

    private void HandleRightClick()
    {
        if (_currentSelection.Count > 0)
        {
            RaycastHit _hit = UnderMouse();
            Selectable _hitSelectable = _hit.transform.GetComponent<Selectable>();
            CollectableResource _collectableResource;
            Building _building;
            Unit _unit;

            if (_hitSelectable != null)
            {
                _collectableResource = _hitSelectable.GetComponent<CollectableResource>();
                _building = _hitSelectable.GetComponent<Building>();
                _unit = _hitSelectable.GetComponent<Unit>();

                if (_collectableResource != null) GiveCollectResourceOrder(_collectableResource);

                if (_building != null && _building.PlayerNumber() != _playerNumber) GiveAttackOrder(_building);

                if (_unit != null && _unit.PlayerNumber() != _playerNumber) GiveAttackOrder(_unit);

                if (_building != null && _building.PlayerNumber() == _playerNumber) GiveUnitToBuildingOrder(_building);
            }
            else
            {
                GiveMoveOrder(_hit.point);
            }


            
        }
    }

    private void GiveUnitToBuildingOrder(Building _building)
    {
        if (_building.IsResourceDropPoint())
        {
            foreach (Selectable _selectable in _currentSelection)
            {
                Unit _unit = _selectable.GetComponent<Unit>();
                if (_unit != null && _unit.IsCarryingResource())
                {
                    _unit.SetResourceDropOffPoint(_building);
                }
            }
        }

        if (_building.BuildState() == Building.EBuildState.Building)
        {
            foreach (Selectable _selectable in _currentSelection)
            {
                BuildingConstructor _constructor = _selectable.GetComponent<BuildingConstructor>();
                if (_constructor != null)
                {
                    _constructor.SetBuildTarget(_building);
                }
            }
        }
    }

    private RaycastHit UnderMouse()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;
        Physics.Raycast(_ray, out _hit);

        return _hit;
    }

    public Vector3 LocationUnderMouse()
    {
        return UnderMouse().point;
    }

    private void ClearSelection()
    {
        if (_currentSelection.Count <= 0) return;

        foreach (Selectable _selectable in _currentSelection)
        {
            _selectable.Selected(false);
        }
        _currentSelection.Clear();
        _hudManager.ClearSelection();
    }

    private void GiveMoveOrder(Vector3 _newLocation)
    {
        foreach (Selectable _selectable in _currentSelection)
        {
            Unit _unit = _selectable.GetComponent<Unit>();
            if (_unit != null)
            {
                _unit.MoveTo(_newLocation);
            }
        }
    }

    private void GiveAttackOrder(Selectable _newTarget)
    {
        foreach (Selectable _selectable in _currentSelection)
        {
            Unit _unit = _selectable.GetComponent<Unit>();
            if (_unit != null && _unit.UnitType() != Unit.EUnitType.Worker)
            {
                _unit.SetTarget(_newTarget);
            }
            else
            {
                Building _building = _selectable.GetComponent<Building>();
                if (_building != null)
                {

                }
            }
        }
    }

    private void GiveCollectResourceOrder(CollectableResource _newResource)
    {
        foreach (Selectable _selectable in _currentSelection)
        {
            Unit _unit = _selectable.GetComponent<Unit>();
            if (_unit != null && _unit.UnitType() == Unit.EUnitType.Worker)
            {
                _unit.SetTarget(_newResource);
            }
        }
    }

    public void SetHUD_Manager(HUD_Manager _newManager)
    {
        _hudManager = _newManager;
    }

    public void PlayerControl(bool _online)
    {
        _playerControlOnline = _online;
    }
}
// Writen by Lukasz Dziedziczak