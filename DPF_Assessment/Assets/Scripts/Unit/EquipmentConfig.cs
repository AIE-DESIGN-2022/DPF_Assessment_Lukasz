// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Equipment")]
public class EquipmentConfig : ScriptableObject
{
    [SerializeField] private GameObject _equipmentPrefab;
    [SerializeField] private bool _heldInLeftHand = false;
    [SerializeField] private AnimatorOverrideController _animatorOverrideController;
    [SerializeField] private Projectile _projectilePrefab;

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

    public bool HasProjectile()
    {
        if (_projectilePrefab == null) return false;
        else return true;
    }

    public Projectile Projectile()
    {
        if (_projectilePrefab != null) return _projectilePrefab;
        else return null;
    }

    public AnimatorOverrideController AnimatorOverrideController()
    {
        if (_animatorOverrideController != null) return _animatorOverrideController;
        else return null;
    }
}
// Writen by Lukasz Dziedziczak