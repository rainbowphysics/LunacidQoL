using System;
using UnityEngine;

namespace LunacidQoL;

public class CollideOnce : MonoBehaviour
{
    public Damage_Trigger dt;
    
    private SphereCollider _collider;

    protected void Start()
    {
        _collider = GetComponent<SphereCollider>();
    }

    protected void OnCollisionEnter(Collision _other)
    {
        Destroy(_collider);
    }
}