using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Equipment")]
public class EquipmentConfig : ScriptableObject
{
    [SerializeField] private GameObject _equipmentPrefab;
    [SerializeField] private bool _heldInLeftHand = false;
    [SerializeField] private AnimatorOverrideController _animatorOverrideController;

    public GameObject Spawn(Unit _unit)
    {
        Transform _handTransform = _unit.HandTransform(_heldInLeftHand);
        GameObject _newObject = Instantiate(_equipmentPrefab, _handTransform.position, _handTransform.rotation);
        _newObject.transform.parent = _handTransform;
        return _newObject;
    }

    public bool HeldInLeftHand()
        { return _heldInLeftHand; }

    public GameObject Prefab() { return _equipmentPrefab; }

}
