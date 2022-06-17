// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator)), RequireComponent(typeof(Health))]
public class Unit : Selectable
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private ResourceGatherer resourceGatherer;
    private BuildingConstructor buildingConstructor;
    private Attacker attacker;
    private bool hasStopped = true;
    private Healer healer;

    [Header("Unit Config")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private EUnitType unitType;

    [Header("Behaviour")]
    [SerializeField] private EUnitStance unitStance = EUnitStance.Passive;

    private bool selectingPatrolPoint = false;
    private Vector3 patrolStartPoint;
    private Vector3 patrolEndPoint;
    private int currentPatrolPoint = 0;
    private EUnitStance previousStance;

    public enum EUnitType
    {
        Worker,
        Melee,
        Ranged,
        Magic,
        Healer
    }

    public enum EUnitStance
    {
        Passive,
        Defensive,
        Offensive,
        Patrol
    }

    private new void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        resourceGatherer = GetComponent<ResourceGatherer>();
        attacker = GetComponent<Attacker>();
        buildingConstructor = GetComponent<BuildingConstructor>();
        healer = GetComponent<Healer>();
        if (leftHand == null) leftHand = FindByName("hand_l");
        if (rightHand == null) rightHand = FindByName("hand_r");
    }

    private new void Start()
    {
        base.Start();
        if (navMeshAgent != null) navMeshAgent.updateRotation = false;
        if (animator != null && animator.runtimeAnimatorController == null) Debug.LogError(name + " missing runtime Animator Controller");

        if (leftHand == null) Debug.LogError(name + " missing left hand transform.");
        if (rightHand == null) Debug.LogError(name + " missing right hand transform.");

    }

    private new void Update()
    {
        base.Update();
        UpdateAnimation();
        PatrolPointSelection();
        PatrolAction();
    }

    private void PatrolPointSelection()
    {
        if (selectingPatrolPoint && gameController.CameraController().MouseIsInPlayArea())
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectingPatrolPoint = false;
                patrolStartPoint = transform.position;
                patrolEndPoint = gameController.PlayerController().LocationUnderMouse();

                unitStance = EUnitStance.Patrol;

                gameController.PlayerController().SettingPatrolPoint(false);
                
                HUD_StatusUpdate();
                Move(patrolEndPoint);
            }

            if (Input.GetMouseButtonDown(1))
            {
                gameController.PlayerController().SettingPatrolPoint(false);
                selectingPatrolPoint = false;
            }
        }
    }

    private void PatrolAction()
    {
        if (unitStance == EUnitStance.Patrol)
        {
            if (currentPatrolPoint == 0)
            {
                float Distance = Vector3.Distance(transform.position, patrolEndPoint);
                if (Distance < 1.0f)
                {
                    currentPatrolPoint = 1;
                    Move(patrolStartPoint);
                }
            }

            if (currentPatrolPoint == 1)
            {
                float Distance = Vector3.Distance(transform.position, patrolStartPoint);
                if (Distance < 1.0f)
                {
                    currentPatrolPoint = 0;
                    Move(patrolEndPoint);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
        }
    }

    private void UpdateAnimation()
    {
        if (animator != null && navMeshAgent != null)
        {
            Vector3 velocity = transform.InverseTransformDirection(navMeshAgent.velocity);
            animator.SetFloat("speed", velocity.z);

            if (!hasStopped && DistanceToNavMeshTarget() < 1 && velocity.z < 0.5f)
            {
                hasStopped = true;
                if (IsSelected()) HUD_StatusUpdate();
            }
        }
    }

    public void MoveTo(Vector3 newLocation)
    {
        //Debug.Log(name + "Recieved MoveTo order");
        ClearPreviousActions();
        Move(NearestEmptyPosition(newLocation));
        HUD_StatusUpdate();
    }

    public void ClearPreviousActions()
    {
        StopMoveTo();
        if (resourceGatherer != null)
        {
            resourceGatherer.ClearTargetResource();
            resourceGatherer.ClearDropOffPoint();
        }
        if (buildingConstructor != null) buildingConstructor.ClearBuildTarget();
        if (attacker != null) attacker.ClearTarget();
        if (healer != null) healer.ClearTargetUnit();
        if (animator != null) animator.SetTrigger("stop");
        ClearPatrol();
    }

    public void MoveTo(Selectable selectable)
    {
        Move(selectable.transform.position);
    }

    public void MoveTo(Building building)
    {
        Move(building.transform.position);
    }

    private void Move(Vector3 newLocation)
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(newLocation, out hit, 3, NavMesh.AllAreas)) return;

        if (navMeshAgent != null)
        {
            
            navMeshAgent.destination = GetClosestAvailablePosition(hit.position);

            if (navMeshAgent.isStopped) navMeshAgent.isStopped = false;
        }

        hasStopped = false;
    }

    public void SetTarget(CollectableResource newCollectableResource)
    {
        ClearPreviousActions();
        if (resourceGatherer != null)
        {
            resourceGatherer.SetTargetResource(newCollectableResource);
        }
    }

    public void SetTarget(Selectable newTarget)
    {
        ClearPreviousActions();
        if (attacker != null)
        {
            attacker.SetTarget(newTarget);
        }

        
    }

    public void SetHealTarget(Unit newTarget)
    {
        ClearPreviousActions();
        if (healer != null && PlayerNumber() == newTarget.PlayerNumber())
        {
            healer.SetTargetHealUnit(newTarget);
        }
    }

    public Animator Animator() { return animator; }

    public void StopMoveTo()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.destination = transform.position;
        }
    }

    public Transform HandTransform(bool requestingLeftHand)
    {
        if (requestingLeftHand && leftHand != null) return leftHand;
        else if (!requestingLeftHand && rightHand != null) return rightHand;
        else return null;
    }

    public EUnitType UnitType() { return unitType; }

    private Transform FindByName(string name)
    {
        Transform returnTransform = null;
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                returnTransform = child;
                break;
            }

        }
        return returnTransform;
    }

    public bool IsCarryingResource()
    {
        if (resourceGatherer != null)
        {
            return resourceGatherer.GatheredResourcesAmount() > 0;
        }
        else
        {
            return false;
        }

    }

    public void SetResourceDropOffPoint(Building newDropPoint)
    {
        if (resourceGatherer != null)
        {
            resourceGatherer.SetTargetDropOffPoint(newDropPoint);
        }
    }

    public void Status(out string status1, out string status2)
    {
        status1 = "";
        status2 = "";

        if (resourceGatherer != null)
        {
            if (resourceGatherer.HasResourceTarget())
            {
                status1 = "Gathering resources";
            }
            else if (resourceGatherer.HasDropOffTarget())
            {
                status1 = "Dropping off resources";
            }
            else if (resourceGatherer.GatheredResourcesAmount() > 0)
            {
                status1 = "Carrying resources";
            }

            if (resourceGatherer.GatheredResourcesAmount() > 0)
            {
                status2 = resourceGatherer.GatheredResourcesAmount().ToString() + " " + resourceGatherer.GatheredResourceType().ToString();
            }
        }
        
        if(buildingConstructor != null)
        {
            if (buildingConstructor.IsConstructingBuilding())
            {
                status1 = "Constructing " + buildingConstructor.CurrentlyBuildingName() + "...";
            }
            else if (buildingConstructor.HasBuildTarget())
            {
                status1 = "Moving to construction.";
            }
        }
        
        if(attacker != null)
        {
            if (attacker.HasTarget())
            {
                if (attacker.TargetIsInRange())
                {
                    status1 = "Attacking";
                }
                else
                {
                    status1 = "Moving to target";
                }
            }
        }

        if (status1 == "" && unitStance == EUnitStance.Patrol)
        {
            status1 = "Patroling";
        }
        else if (status1 == "" && DistanceToNavMeshTarget() > 1)
        {
            status1 = "Moving";
        }
        else if (status1 == "")
        {
            status1 = "Idle";
        }
    }

    private float DistanceToNavMeshTarget()
    {
        if (navMeshAgent.destination != null)
        {
            return Vector3.Distance(navMeshAgent.destination, transform.position);
        }
        else return Mathf.Infinity;
    }

    public void TakeAStepBack()
    {
        Vector3 newPosition = transform.position + (transform.forward * -1);
        MoveTo(newPosition);
    }

    private Vector3 GetClosestAvailablePosition(Vector3 newPosition)
    {
        Vector3 position = newPosition;
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(newPosition, out navMeshHit, 5, NavMesh.AllAreas))
        {
            position = navMeshHit.position;
        }

        return position;
    }

    private bool CheckIfNewPositionEmpty(Vector3 newPosition)
    {
        RaycastHit[] hits = Physics.SphereCastAll(newPosition, 0.5f, Vector3.up, 0.5f, NavMesh.AllAreas);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<Unit>() != null) return false;
        }

        return true;
    }

    private Vector3 NearestEmptyPosition(Vector3 newPosition)
    {
        Vector3 position = newPosition;
        int itereation = 1;

        while (!CheckIfNewPositionEmpty(position))
        {
            float randX = UnityEngine.Random.Range(-1.0f * itereation, 1.0f * itereation);
            float randZ = UnityEngine.Random.Range(-1.0f * itereation, 1.0f * itereation);

            position = new Vector3(newPosition.x + randX, newPosition.y, newPosition.z + randZ);
            itereation++;
        }

        return position;
    }

    public void ChangeUnitStance(EUnitStance newStance)
    {
        if (newStance == EUnitStance.Patrol)
        {
            previousStance = unitStance;
            SetPatrol();
        }
        unitStance = newStance;
    }

    public EUnitStance UnitStance() {  return unitStance; }

    public List<Unit> GetFrendlyUnitsInSight()
    {
        List<Unit> list = new List<Unit>();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sightDistance, Vector3.up);
        foreach (RaycastHit hit in hits)
        {
            Unit unit = hit.transform.GetComponent<Unit>();

            if (unit != null && unit.PlayerNumber() == PlayerNumber()) list.Add(unit);
        }

        return list;
    }

    public void SetPatrol()
    {
        gameController.PlayerController().SettingPatrolPoint(true);
        selectingPatrolPoint = true;
    }

    public void ClearPatrol()
    {
        if (unitStance == EUnitStance.Patrol)
        {
            unitStance = previousStance;
            patrolStartPoint = new Vector3();
            patrolEndPoint = new Vector3();
        }

        if (IsSelected()) gameController.HUD_Manager().Actions_HUD().UpdateUnitStances();
    }

    public bool HasStopped() { return hasStopped; }

    public bool NeedsHealing()
    {
        return health.HealthPercentage() != 1;
    }

    public bool IsIdle()
    {
        if (resourceGatherer != null && resourceGatherer.HasResourceTarget()) return false;
        if (resourceGatherer != null && resourceGatherer.HasDropOffTarget()) return false;
        if (buildingConstructor != null && buildingConstructor.HasBuildTarget()) return false;

        if (hasStopped) return true;
        else return false;
    }
}
// Writen by Lukasz Dziedziczak