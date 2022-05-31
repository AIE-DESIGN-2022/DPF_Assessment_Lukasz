using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] private float healRate = 5;
    [SerializeField] private float healRange = 5;
    [SerializeField] private EquipmentConfig healingHands;
    [SerializeField] private ParticleSystem healEffectPrefab;

    private Unit targetUnit;
    private Unit unit;
    private ParticleSystem healEffect;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        if (unit.Animator() != null && healingHands.AnimatorOverrideController() != null) unit.Animator().runtimeAnimatorController = healingHands.AnimatorOverrideController();

        if (unit.UnitStance() == Unit.EUnitStance.Offensive || unit.UnitStance() == Unit.EUnitStance.Patrol)
        {
            unit.ChangeUnitStance(Unit.EUnitStance.Defensive);
        }
    }

    private void Update()
    {
        HealerLogic();
        DefensiveStanceLogic();
    }

    private void DefensiveStanceLogic()
    {
        if (unit.UnitStance() == Unit.EUnitStance.Defensive)
        {
            if (targetUnit == null)
            {
                List<Unit> visibleUnits = unit.GetFrendlyUnitsInSight();
                foreach (Unit visibleUnit in visibleUnits)
                {
                    if (visibleUnit.NeedsHealing())
                    {
                        targetUnit = visibleUnit;
                        return;
                    }
                }
            }
        }
    }

    private void HealerLogic()
    {
        if (targetUnit != null)
        {
            if (TargetRequiresHealing(targetUnit) && TargetIsInRange())
            {
                HealTarget();
            }
            else
            {
                unit.MoveTo(targetUnit);
            }
        }
    }

    private void HealTarget()
    {
        if (unit != null && unit.Animator() != null)
        {
            unit.StopMoveTo();
            unit.Animator().SetBool("attack", true);
        }

        if (healEffectPrefab != null && healEffect == null)
        {
            healEffect = Instantiate(healEffectPrefab, targetUnit.transform.position, targetUnit.transform.rotation);
            healEffect.transform.parent = targetUnit.transform;

        }
    }

    private bool TargetIsInRange()
    {
        if (targetUnit != null)
        {
            float distance = Vector3.Distance(transform.position, targetUnit.transform.position);
            return distance < healRange;
        }
        return false;
    }

    private bool TargetRequiresHealing(Unit target)
    {
        Health targetUnitHealth = target.GetComponent<Health>();

        if (targetUnitHealth != null & targetUnitHealth.IsFull())
        {
            return false;
        }
        else if ((targetUnitHealth != null & !targetUnitHealth.IsFull()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetTargetHealUnit(Unit newTarget)
    {
        if (TargetRequiresHealing(newTarget))
        {
            targetUnit = newTarget;
        }
    }

    public void ClearTargetUnit()
    {
        targetUnit = null;
        if (unit != null && unit.Animator() != null)
        {
            unit.Animator().SetBool("attack", false);
            unit.Animator().SetTrigger("stop");
        }

        if (healEffect != null)
        {
            Destroy(healEffect.gameObject);
            healEffect = null;
        }
    }

    private void HealEvent()
    {
        if (targetUnit != null)
        {
            targetUnit.Heal(healRate);

            if (!TargetRequiresHealing(targetUnit))
            {
                ClearTargetUnit();
            }
        }
    }
}
