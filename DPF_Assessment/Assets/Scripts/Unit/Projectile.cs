using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _timeToLive = 5;

    private float _lifeTime = 0;
    private float _damage;
    private Unit _owner;
    private bool _flyingInAir = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _lifeTime += Time.deltaTime;

        if (_lifeTime > _timeToLive)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (_flyingInAir) transform.position += transform.forward * Time.deltaTime * _speed;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!_flyingInAir) return;

        Unit _unit = other.transform.GetComponent<Unit>();
        if (_unit != null && _unit != _owner)
        {
            Hit(_unit);
        }
        else
        {
            Building _building = other.transform.GetComponent<Building>();
            if (_building != null)
            {
                Hit(_building);
            }
        }
    }

    private void Hit(Selectable _target)
    {
        _flyingInAir = false;
        _target.TakeDamage(_damage);
        //transform.parent = _target.transform;
        Destroy(gameObject, 3);
    }

    public void Setup(Unit _newOwner, float _newDamage)
    {
        _owner = _newOwner;
        _damage = _newDamage;
    }
}
