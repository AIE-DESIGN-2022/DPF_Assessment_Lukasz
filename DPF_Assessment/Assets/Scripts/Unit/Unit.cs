using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)),RequireComponent(typeof(Animator))]
public class Unit : Selectable
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private ResourceGatherer _resourceGatherer;

    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

    private new void Awake()
    {
        base.Awake();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _resourceGatherer = GetComponent<ResourceGatherer>();
    }

    private new void Start()
    {
        base.Start();
        if (_navMeshAgent != null) _navMeshAgent.updateRotation = false;
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
        }
    }

    public void MoveTo(Vector3 _newLocation)
    {
        if (_resourceGatherer != null) _resourceGatherer.ClearTargetResource();
        Move(_newLocation);
    }

    public void MoveTo(CollectableResource _collectableResource)
    {
        Move(_collectableResource.transform.position);
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
        else Debug.LogError("No Collectable Resource");
    }

    public Animator Animator() { return _animator; }

    public void StopMoveTo()
    {
        if (_navMeshAgent != null) _navMeshAgent.isStopped = true;
    }
}
