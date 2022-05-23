using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator)), RequireComponent(typeof(Health)), RequireComponent(typeof(Rigidbody))]
public class Unit : Selectable
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private ResourceGatherer _resourceGatherer;
    private Attacker _attacker;

    [Header("Unit Config")]
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private EUnitType _unitType;

    public enum EUnitType
    {
        Worker,
        Melee,
        Ranged,
        Magic,
        Healer
    }

    private new void Awake()
    {
        base.Awake();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _resourceGatherer = GetComponent<ResourceGatherer>();
        _attacker = GetComponent<Attacker>();
        if (_leftHand == null) _leftHand = FindByName("hand_l");
        if (_rightHand == null) _rightHand = FindByName("hand_r");
    }

    private new void Start()
    {
        base.Start();
        if (_navMeshAgent != null) _navMeshAgent.updateRotation = false;
        SetupRigidbody();
    }

    private void SetupRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void LateUpdate()
    {
        if (_navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(_navMeshAgent.velocity.normalized);
        }
    }

    private void UpdateAnimation()
    {
        if (_animator != null && _navMeshAgent != null)
        {
            Vector3 velocity = transform.InverseTransformDirection(_navMeshAgent.velocity);
            _animator.SetFloat("speed", velocity.z);

            /*if (IsSelected() &&  velocity.z < 0.5f)
            {
                HUD_StatusUpdate();
            }*/
        }
    }

    public void MoveTo(Vector3 _newLocation)
    {
        if (_resourceGatherer != null)
        {
            _resourceGatherer.ClearTargetResource();
            _resourceGatherer.ClearDropOffPoint();
        }
        if (_attacker != null) _attacker.ClearTarget();
        if (_animator != null) _animator.SetTrigger("stop");
        Move(_newLocation);
        HUD_StatusUpdate();
    }

    public void MoveTo(CollectableResource _collectableResource)
    {
        Move(_collectableResource.transform.position);

    }

    public void MoveTo(Selectable _selectable)
    {
        Move(_selectable.transform.position);
    }

    public void MoveTo(Building _building)
    {
        Move(_building.transform.position);
    }

    private void Move(Vector3 _newLocation)
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.destination = _newLocation;

            if (_navMeshAgent.isStopped) _navMeshAgent.isStopped = false;
        }
    }

    public void SetTarget(CollectableResource _newCollectableResource)
    {
        if (_resourceGatherer != null)
        {
            _resourceGatherer.SetTargetResource(_newCollectableResource);
        }
    }

    public void SetTarget(Selectable _newTarget)
    {
        if (_attacker != null)
        {
            _attacker.SetTarget(_newTarget);
        }
    }

    public Animator Animator() { return _animator; }

    public void StopMoveTo()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.isStopped = true;
            //_navMeshAgent.destination = new Vector3();
        }
    }

    public Transform HandTransform(bool _requestingLeftHand)
    {
        if (_requestingLeftHand && _leftHand != null) return _leftHand;
        else if (!_requestingLeftHand && _rightHand != null) return _rightHand;
        else return null;
    }

    public EUnitType UnitType() { return _unitType; }

    private Transform FindByName(string _name)
    {
        Transform _returnTransform = null;
        Transform[] _children = GetComponentsInChildren<Transform>();
        foreach (Transform _child in _children)
        {
            if (_child.name == _name)
            {
                _returnTransform = _child;
                break;
            }

        }
        return _returnTransform;
    }

    public bool IsCarryingResource()
    {
        if (_resourceGatherer != null)
        {
            return _resourceGatherer.GatheredResourcesAmount() > 0;
        }
        else
        {
            return false;
        }

    }

    public void SetResourceDropOffPoint(Building _newDropPoint)
    {
        if (_resourceGatherer != null)
        {
            _resourceGatherer.SetTargetDropOffPoint(_newDropPoint);
        }
    }

    public void Status(out string _status1, out string _status2)
    {
        _status1 = "";
        _status2 = "";

        if (_resourceGatherer != null)
        {
            if (_resourceGatherer.HasResourceTarget())
            {
                _status1 = "Gathering resources";
            }
            else if (_resourceGatherer.HasDropOffTarget())
            {
                _status1 = "Dropping off resources";
            }
            else if (_resourceGatherer.GatheredResourcesAmount() > 0)
            {
                _status1 = "Carrying resources";
            }

            if (_resourceGatherer.GatheredResourcesAmount() > 0)
            {
                _status2 = _resourceGatherer.GatheredResourcesAmount().ToString() + " " + _resourceGatherer.GatheredResourceType().ToString();
            }
        }
        
        if (_status1 == "" && _navMeshAgent.destination != null && Vector3.Distance(_navMeshAgent.destination, transform.position) > 1)
        {
            _status1 = "Moving";
        }
        else if (_status1 == "")
        {
            _status1 = "Idle";
        }
    }
}
