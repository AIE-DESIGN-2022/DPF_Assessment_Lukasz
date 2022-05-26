using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] private float healRate = 5;
    [SerializeField] private float healRange = 5;
    [SerializeField] private EquipmentConfig healingHands;

    private Unit targetUnit;
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        unit.Animator().runtimeAnimatorController = healingHands.AnimatorOverrideController();
    }

    private void Update()
    {
        HealerLogic();
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
