// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [Tooltip("Weapon used by this unit.")]
    [SerializeField] private EquipmentConfig unitWeapon;
    [Tooltip("Base level of damage given by this unit upon attack.")]
    [SerializeField] private float attackDamage = 10;
    [Tooltip("Time between the start  of each attack in seconds.")]
    [SerializeField] private float attackRate = 1;
    [Tooltip("How close to target the unit has to be for an attack.")]
    [SerializeField] private float attackRange = 1;

    public Selectable target;
    private float timeSinceLastAttack = Mathf.Infinity;
    private Unit unit;
    private Building building;
    private GameObject weapon;
    private bool hasProjectileWeapon;
    private float sightTimer = 0;
    private CombatMultiplier combatMultiplier;
    private bool attackMove = false;

    [Header("Tower Settings")]
    [SerializeField] private Unit.EUnitStance towerStance = Unit.EUnitStance.Defensive;

    private Transform towerSpawn;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();
        combatMultiplier = (CombatMultiplier)Resources.Load<CombatMultiplier>("CombatMultiplier");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (unitWeapon != null) Equip(unitWeapon);
        if (building != null)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.name == "TowerSpawn") towerSpawn = t;
            }
        }
        if (combatMultiplier == null) Debug.LogError(name + " cannot find CombatMultiplier");
    }

    private void Equip(EquipmentConfig newEquipment)
    {
        if (unitWeapon != null)
        {
            
            if (unit != null)
            {
                weapon = unitWeapon.Spawn(unit);
                if (unitWeapon.AnimatorOverrideController() != null) unit.Animator().runtimeAnimatorController = unitWeapon.AnimatorOverrideController();
            }

            if (building != null)
            {
                weapon = unitWeapon.Spawn(building);
            }
            
            
            hasProjectileWeapon = unitWeapon.HasProjectile();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (unit != null && unit.IsAlive())
        {
            if (target != null && target.IsAlive())
            {
                if (TargetIsInRange())
                {
                    AttackTarget();
                }
                else
                {
                    unit.Animator().SetTrigger("stop");
                    unit.MoveTo(target);
                }
            }
            else if (target != null && !target.IsAlive()) ClearTarget();

            if ((unit.UnitStance() == Unit.EUnitStance.Offensive && unit.HasStopped()) || unit.UnitStance() == Unit.EUnitStance.Patrol)
            {
                AttackEnemiesInRange();
            }

            if (attackMove && target == null)
            {
                AttackEnemiesInRange();

                if (target == null)
                {
                    unit.ContinueToDestination();
                }
            }
        }

        if (building != null && building.ConstructionComplete() && building.IsAlive())
        {
            if (towerStance == Unit.EUnitStance.Defensive)
            {
                AttackEnemiesInRange();
            }

            if (target != null && !TargetIsInRange())
            {
                ClearTarget();
            }

            if (target != null && TargetIsInRange())
            {
                AttackTarget();
            }
        }
    }

    private void AttackEnemiesInRange()
    {
        sightTimer += Time.deltaTime;

        if (target == null && sightTimer > 0.5f)
        {
            sightTimer = 0;
            List<Selectable> enemiesInSight = null;

            if (unit != null) enemiesInSight = unit.GetEnemiesInSight();
            else if (building != null) enemiesInSight = building.GetEnemiesInSight();

            if (enemiesInSight != null && enemiesInSight.Count == 1) SetTarget(enemiesInSight[0]);
            if (enemiesInSight != null && enemiesInSight.Count > 1)
            {
                SetTarget(ClosestTarget(enemiesInSight));
            }
        }
    }

    private void AttackTarget()
    {
        if (timeSinceLastAttack > attackRate)
        {
            timeSinceLastAttack = 0;

            if (unit != null)
            {
                unit.StopMoveTo();
                transform.LookAt(target.transform);
                unit.Animator().SetTrigger("attack");
                unit.HUD_StatusUpdate();
            }
             if (building != null)
            {
                if (towerSpawn) towerSpawn.LookAt(target.transform);
                AttackEffect();
            }
        }
    }

    public bool TargetIsInRange()
    {
        if (target != null)
        {
            float offset = 0;
            Building targetBuilding = target.GetComponent<Building>();
            if (targetBuilding)
            {
                offset = targetBuilding.RangeOffset();
            }
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance < attackRange + offset;
        }
        else return false;
    }

    private bool TargetIsInSight()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance < building.SightDistance();
        }
        else return false;
    }

    public bool HasTarget() { return target != null; }

    public void SetTarget(Selectable newTarget)
    {
        target = newTarget;

        if (unit != null)
        {
            unit.HUD_StatusUpdate();
        }
        else if (building != null)
        {
            building.HUD_StatusUpdate();
        }
    }

    public void ClearTarget()
    {
        target = null;

        if (unit != null)
        {
            unit.Animator().SetTrigger("stop");
            unit.HUD_StatusUpdate();
        }
        else if (building != null)
        {
            building.HUD_StatusUpdate();
        } 
    }

    // Function called by Animation at the point impact on target occurs.
    private void AttackEffect()
    {
        if (target != null)
        {
            if (hasProjectileWeapon)
            {
                Transform spawnLocation;

                if (unit != null)
                {
                    spawnLocation = transform;
                }
                else if (building != null)
                {
                    spawnLocation = towerSpawn;
                }
                else
                {
                    spawnLocation = null;
                }

                Vector3 spawnPosition = spawnLocation.position;
                spawnPosition.y += 1;
                spawnPosition += spawnLocation.forward;
                Projectile projectile = Instantiate(unitWeapon.Projectile(), spawnPosition, spawnLocation.rotation);

                if (unit != null)
                {
                    projectile.Setup(unit, attackDamage);
                }
                else if (building != null)
                {
                    projectile.Setup(building, attackDamage);
                }
                
            }
            else
            {
                float attackMultiplier = 1;
                if (unit != null) attackMultiplier = combatMultiplier.GetMultiplier(unit, target);
                else if (building != null) attackMultiplier = combatMultiplier.GetMultiplier(building, target);
                //print(name + " giving damage=" + attackDamage + " multiplier=" + attackMultiplier + " total=" + attackDamage * attackMultiplier);
                target.TakeDamage(attackDamage * attackMultiplier, unit);
                if (!target.IsAlive()) ClearTarget();
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

    public void ChangeTowerStance(Unit.EUnitStance newStance)
    {
        towerStance = newStance;
    }

    public Unit.EUnitStance TowerStance()
    {
        return towerStance;
    }

    public void IsAttackMove(bool isAttackMove)
    {
        attackMove = isAttackMove;
    }

    public bool CurrentlyAttackMove { get { return attackMove; } }
}
// Writen by Lukasz Dziedziczak