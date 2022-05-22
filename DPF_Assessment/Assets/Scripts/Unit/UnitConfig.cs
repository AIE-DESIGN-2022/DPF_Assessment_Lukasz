using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="RTS/Unit")]
public class UnitConfig : ScriptableObject
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Unit unitPrefabAlt;
    [SerializeField] private float _buildTime;

    public float BuildTime() { return _buildTime; }

    public Unit Spawn(int _playerNumber, Transform _spawnLocation)
    {
        Unit _unit = Instantiate(GetPrefab(), _spawnLocation.position, _spawnLocation.rotation);
        _unit.SetPlayerNumber(_playerNumber);
        _unit.transform.parent = FindObjectOfType<GameController>().GetFaction(_playerNumber).transform;
        return _unit;
    }

    private Unit GetPrefab()
    {
        if (unitPrefab != null && unitPrefabAlt != null)
        {
            bool _useAlt = Random.Range(0, 1) == 0;
            if (_useAlt)  return unitPrefabAlt;
            else return unitPrefab;
        }
        else if (unitPrefab != null) return unitPrefab;
        else if (unitPrefabAlt != null) return unitPrefabAlt;
        else return null;
    }
}
