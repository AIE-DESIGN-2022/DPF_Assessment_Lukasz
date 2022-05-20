using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Equipment")]
public class EquipmentConfig : ScriptableObject
{
    [SerializeField] private GameObject _equipmentPrefab;
    [SerializeField] private bool _heldInLeftHand = false;

    public GameObject Spawn(Transform handTransform)
    {
        GameObject _newObject = Instantiate(_equipmentPrefab, handTransform);
        _newObject.transform.parent = handTransform;
        return _newObject;
    }

    public bool HeldInLeftHand()
        { return _heldInLeftHand; }

}
