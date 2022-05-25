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
    private RectTransform _selectionBox;
    private Vector2 _selectionPoint1;
    private Vector2 _selectionPoint2;


    private void Awake()
    {
        _cameraController = FindObjectOfType<GameCameraController>();
        _selectionBox = FindSelectionBox();
    }

    private RectTransform FindSelectionBox()
    {
        RectTransform[] _rectTrans = FindObjectsOfType<RectTransform>();
        foreach (RectTransform _rectTran in _rectTrans)
        {
            if (_rectTran.name == "SelectionBox") return _rectTran;
        }

        Debug.LogError(name + " unable to find SelectionBox.");
        return null;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _currentSelection = new List<Selectable>();
        _selectionBox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_cameraController.IsInUIOffset()) return;
        if (!_playerControlOnline) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();

        if (Input.GetMouseButton(0)) HandleLeftMouseDown();

        if (Input.GetMouseButtonUp(0)) HandleLeftMouseUp();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();

        RaycastHit _hit = UnderMouse();
        Selectable _selectable = _hit.transform.GetComponent<Selectable>();
        if (_selectable != null && _selectable.PlayerNumber() == _playerNumber)
        {
            AddToSelection(_selectable);
        }

        if (_selectable != null && _currentSelection.Count == 0)
        {
            AddToSelection(_selectable);
        }

        if (_selectable == null) // if no selectable object was hit with mouse click
        {
            _selectionPoint1 = Input.mousePosition;
        }
    }

    private void AddToSelection(Selectable _selectable)
    {
        _selectable.Selected(true);
        _currentSelection.Add(_selectable);
        _hudManager.NewSelection(_currentSelection);
    }

    private void AddToSelection(List<Selectable> _selectables)
    {
        foreach (Selectable _selectable in _selectables)
        {
            _selectable.Selected(true);
            _currentSelection.Add(_selectable);
        }
        _hudManager.NewSelection(_currentSelection);
    }

    private void HandleLeftMouseDown()
    {
        if (_selectionPoint1 == new Vector2()) return;
        _selectionPoint2 = Input.mousePosition;

        if (Vector3.Distance(_selectionPoint1, _selectionPoint2) > 2.0f && _cameraController.MouseIsInPlayArea())
        {
            _selectionBox.gameObject.SetActive(true);
            _cameraController.allowMovement = false;

            float _width = _selectionPoint2.x - _selectionPoint1.x;
            float _height = _selectionPoint2.y - _selectionPoint1.y;

            _selectionBox.sizeDelta = new Vector2(Mathf.Abs(_width), Mathf.Abs(_height));
            _selectionBox.anchoredPosition = _selectionPoint1 + new Vector2(_width/2 , _height/2);
        }
    }

    private void HandleLeftMouseUp()
    {
        if (_selectionBox.gameObject.activeInHierarchy)
        {
            AddToSelection(PlayersUnits(InSelectionBox()));

            _selectionBox.gameObject.SetActive(false);
            _cameraController.allowMovement = true;
        }
        _selectionPoint1 = new Vector2();
        _selectionPoint2 = new Vector2();
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

    private List<Selectable> InSelectionBox()
    {
        List<Selectable> _list = new List<Selectable>();

        Vector2 min = _selectionBox.anchoredPosition - (_selectionBox.sizeDelta / 2);
        Vector2 max = _selectionBox.anchoredPosition + (_selectionBox.sizeDelta / 2);

        Selectable[] _allSelectables = GameObject.FindObjectsOfType<Selectable>();
        foreach (Selectable _selectable in _allSelectables)
        {
            Vector3 _screenPos = _cameraController.Camera().WorldToScreenPoint(_selectable.transform.position);
            if (_screenPos.x > min.x && _screenPos.x < max.x && _screenPos.y > min.y && _screenPos.y < max.y)
            {
                _list.Add(_selectable);
            }
        }

        return _list;
    }

    private List<Selectable> PlayersUnits(List<Selectable> _selectables)
    {
        List<Selectable> _list = new List<Selectable>();

        foreach (Selectable _selectable in _selectables)
        {
            if (_selectable.PlayerNumber() == _playerNumber)
            {
                Unit _unit = _selectable.GetComponent<Unit>();
                if (_unit != null) _list.Add(_unit);
            }
        }

        return _list;
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
            if (_unit != null && _unit.PlayerNumber() == _playerNumber)
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
            if (_unit != null && _unit.PlayerNumber() == _playerNumber && _unit.UnitType() != Unit.EUnitType.Worker)
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