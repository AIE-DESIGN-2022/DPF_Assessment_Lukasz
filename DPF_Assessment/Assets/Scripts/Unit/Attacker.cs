using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [SerializeField] private EquipmentConfig _unitWeapon;
    [SerializeField] private float _attackDamage = 10;
    [SerializeField] private float _attackRate = 1;
    [SerializeField] private float _attackRange = 1;

    private Selectable _target;
    private float _timeSinceLastAttack = Mathf.Infinity;
    private Unit _unit;
    private GameObject _weapon;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon();
    }

    private void EquipWeapon()
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastAttack += Time.deltaTime;

        if (_target != null)
        {

            if (TargetIsInRange())
            {
                _unit.StopMoveTo();
                AttackTarget();
            }
            else
            {
                _unit.MoveTo(_target);
            }
        }
    }

    private void AttackTarget()
    {
        throw new NotImplementedException();
    }

    private bool TargetIsInRange()
    {
        throw new NotImplementedException();
    }
}
