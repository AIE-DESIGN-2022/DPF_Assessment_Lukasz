// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [Tooltip("Weapon used by this unit.")]
    [SerializeField] private EquipmentConfig _unitWeapon;
    [Tooltip("Base level of damage given by this unit upon attack.")]
    [SerializeField] private float _attackDamage = 10;
    [Tooltip("Time between the start  of each attack in seconds.")]
    [SerializeField] private float _attackRate = 1;
    [Tooltip("How close to target the unit has to be for an attack.")]
    [SerializeField] private float _attackRange = 1;

    private Selectable _target;
    private float _timeSinceLastAttack = Mathf.Infinity;
    private Unit _unit;
    private Building building;
    private GameObject _weapon;
    private bool _hasProjectileWeapon;
    private float sightTimer = 0;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        building = GetComponent<Building>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_unitWeapon != null) Equip(_unitWeapon);
    }

    private void Equip(EquipmentConfig _newEquipment)
    {
        if (_unitWeapon != null && _unit != null)
        {
            _weapon = _unitWeapon.Spawn(_unit);
            _hasProjectileWeapon = _unitWeapon.HasProjectile();
            if (_unitWeapon.AnimatorOverrideController() != null) _unit.Animator().runtimeAnimatorController = _unitWeapon.AnimatorOverrideController();
        }
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastAttack += Time.deltaTime;

        if (_target != null && _target.IsAlive())
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
        else if (_target != null && !_target.IsAlive()) ClearTarget();

        if (_unit != null)
        {
            if ((_unit.UnitStance() == Unit.EUnitStance.Offensive && _unit.HasStopped()) || _unit.UnitStance() == Unit.EUnitStance.Patrol)
            {
                sightTimer += Time.deltaTime;

                if (_target == null && sightTimer > 0.5f)
                {
                    //print(name + " running aggressive behaviour");
                    sightTimer = 0;
                    List<Selectable> enemiesInSight = _unit.GetEnemiesInSight();
                    //print(enemiesInSight.Count);
                    if (enemiesInSight.Count == 1) SetTarget(enemiesInSight[0]);
                    if (enemiesInSight.Count > 1)
                    {
                        SetTarget(ClosestTarget(enemiesInSight));
                    }
                    //print(name + "set target of " + _target);
                }
            }
        }

        
    }

    private void AttackTarget()
    {
        if (_timeSinceLastAttack > _attackRate)
        {
            transform.LookAt(_target.transform);
            _unit.Animator().SetTrigger("attack");
            _timeSinceLastAttack = 0;
            _unit.HUD_StatusUpdate();
        }
    }

    public bool TargetIsInRange()
    {
        if (_target != null)
        {
            float offset = 0;
            Building targetBuilding = _target.GetComponent<Building>();
            if (targetBuilding)
            {
                offset = targetBuilding.RangeOffset();
            }
            float _distance = Vector3.Distance(transform.position, _target.transform.position);
            return _distance < _attackRange + offset;
        }
        else return false;
    }

    public bool HasTarget() { return _target != null; }

    public void SetTarget(Selectable _newTarget)
    {
        _target = _newTarget;
        _unit.HUD_StatusUpdate();
    }

    public void ClearTarget()
    {
        _target = null;
        _unit.Animator().SetTrigger("stop");
        _unit.HUD_StatusUpdate();
    }

    // Function called by Animation at the point impact on target occurs.
    private void AttackEffect()
    {
        if (_target != null)
        {
            if (_hasProjectileWeapon)
            {
                Vector3 spawnLocation = transform.position;
                spawnLocation.y += 1;
                spawnLocation += transform.forward;
                Projectile projectile = Instantiate(_unitWeapon.Projectile(), spawnLocation, transform.rotation);
                projectile.Setup(_unit, _attackDamage);
            }
            else
            {
                _target.TakeDamage(_attackDamage, _unit);
                if (!_target.IsAlive()) ClearTarget();
            }
        }
    }

    private Selectable ClosestTarget(List<Selectable> selectables)
    {
        Selectable closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Selectable selectable in selectables)
        {
            float distance = Vector3.Distance(transform.position, selectable.transform.position);
            if (distance < closestDistance)
            {
                closest = selectable;
                closestDistance = distance;
            }
        }

        return closest;
    }
}
// Writen by Lukasz Dziedziczak