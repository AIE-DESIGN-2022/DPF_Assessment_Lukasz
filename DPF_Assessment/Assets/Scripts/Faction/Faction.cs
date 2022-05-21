using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private EFaction _faction;
    [SerializeField] private int _playerNumber;

    private int _wood;
    private int _food;
    private int _gold;

    private List<Unit> _units;
    private List<Building> _buildings;

    public enum EFaction
    {
        Human,
        Goblin
    }

    // Start is called before the first frame update
    void Start()
    {
        NameFaction();
        SetChildrenPlayerNumber();
    }

    private void SetChildrenPlayerNumber()
    {
        Selectable[] children = GetComponentsInChildren<Selectable>();
        foreach (Selectable child in children)
        {
            child.SetPlayerNumber(_playerNumber);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupFaction(EFaction _new, int _playerNo)
    {
        _faction = _new;
        _playerNumber = _playerNo;
        NameFaction();
        
    }

    private void NameFaction()
    {
        gameObject.name = "Faction(" + _playerNumber + ")_" + _faction.ToString();
    }    
}
