using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private bool _newGame;

    private List<Faction> _factions;

    // Start is called before the first frame update
    void Start()
    {
        if (_newGame) SetupNewGame();
        else SetupNewGame();
    }

    private void SetupNewGame()
    {

    }

    private void SetupGame()
    {
        Faction[] _newFactions = FindObjectsOfType<Faction>();
        foreach (Faction _faction in _newFactions)
        {
            _factions.Add(_faction);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddFaction()
    {
        GameObject _new = Instantiate(new GameObject(), transform);
        _new.AddComponent<Faction>();
        Faction _faction = _new.GetComponent<Faction>();
        //_faction.SetFaction();
    }

    public Faction GetFaction(int _playerNumber)
    {
        foreach (Faction _faction in _factions)
        {
            if (_faction.PlayerNumber() == _playerNumber)
            {
                return _faction;
            }
        }

        return null;
    }
}
