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
        if (_unitWeapon != null && _unit != null)
        {
            _weapon = _unitWeapon.Spawn(_unit.HandTransform(_unitWeapon.HeldInLeftHand()));
        }
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
        if (_timeSinceLastAttack > _attackRate)
        {
            _unit.Animator().SetTrigger("attack");
            _timeSinceLastAttack = 0;
        }
    }

    private bool TargetIsInRange()
    {
        if (_target != null)
        {
            float _distance = Vector3.Distance(transform.position, _target.transform.position);
            return _distance < _attackRange;
        }
        else return false;
    }

    public void SetTarget(Selectable _newTarget)
    {
        _target = _newTarget;
    }

    public void ClearTarget()
    {
        _target = null;
    }
}
