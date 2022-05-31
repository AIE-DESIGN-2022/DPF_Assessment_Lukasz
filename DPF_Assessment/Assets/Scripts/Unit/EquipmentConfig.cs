// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Equipment")]
public class EquipmentConfig : ScriptableObject
{
    [SerializeField] private GameObject equipmentPrefab;
    [SerializeField] private bool heldInLeftHand = false;
    [SerializeField] private AnimatorOverrideController animatorOverrideController;
    [SerializeField] private Projectile projectilePrefab;

    public GameObject Spawn(Unit unit)
    {
        if (equipmentPrefab != null)
        {
            Transform handTransform = unit.HandTransform(heldInLeftHand);
            GameObject newObject = Instantiate(equipmentPrefab, handTransform.position, handTransform.rotation);
            newObject.transform.parent = handTransform;
            return newObject;
        }
        else
        {
            return null;
        }
        
    }

    public GameObject Spawn(Building building)
    {
        if (equipmentPrefab != null)
        {
            GameObject newObject = Instantiate(equipmentPrefab, building.transform.position, building.transform.rotation);
            newObject.transform.parent = building.transform;
            return newObject;
        }
        else
        {
            return null;
        }
    }

    public bool HeldInLeftHand()
        { return heldInLeftHand; }

    public GameObject Prefab() { return equipmentPrefab; }

    public bool HasProjectile()
    {
        if (projectilePrefab == null) return false;
        else return true;
    }

    public Projectile Projectile()
    {
        if (projectilePrefab != null) return projectilePrefab;
        else return null;
    }

    public AnimatorOverrideController AnimatorOverrideController()
    {
        if (animatorOverrideController != null) return animatorOverrideController;
        else return null;
    }
}
// Writen by Lukasz Dziedziczak