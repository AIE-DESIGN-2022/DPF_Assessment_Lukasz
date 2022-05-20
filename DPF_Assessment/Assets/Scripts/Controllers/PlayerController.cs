using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private List<Selectable> _currentSelection;

    // Start is called before the first frame update
    private void Start()
    {
        _currentSelection = new List<Selectable>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();

    }

    private void HandleLeftClick()
    {
        ClearSelection();

        RaycastHit _hit = UnderMouse();
        Selectable _selectable = _hit.transform.GetComponent<Selectable>();
        if (_selectable != null)
        {
            _selectable.Selected(true);
            _currentSelection.Add(_selectable);
        }
    }

    private void HandleRightClick()
    {
        if (_currentSelection.Count > 0)
        {
            RaycastHit _hit = UnderMouse();
            foreach (Selectable _selectable in _currentSelection)
            {
                Unit _unit = _selectable.GetComponent<Unit>();
                if (_unit != null) _unit._movement.MoveTo(_hit.point);
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

    private void ClearSelection()
    {
        if (_currentSelection.Count <= 0) return;

        foreach (Selectable _selectable in _currentSelection)
        {
            _selectable.Selected(false);
        }
        _currentSelection.Clear();
    }
}
