using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace LunacidQoL;

public class Raycheck : MonoBehaviour
{
    public ManualLogSource Logger;
    
    private Rigidbody _rb;
    private Damage_Trigger _dt;
    private bool _doCheck;

    private RaycastHit[] _raycastHits = new RaycastHit[8];
    
    protected void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        _dt = GetComponent<Damage_Trigger>();
        if (_dt != null && !_dt.Constant)
        {
            _dt.last = Time.time - Mathf.Epsilon;
            StartCoroutine(BeginChecks());
        }
    }

    protected IEnumerator BeginChecks()
    {
        // Don't begin checks immediately, but instead wait for all components to be initialized
        yield return new WaitForEndOfFrame();
        
        // Then wait for the projectile to advance one physics timestep
        yield return new WaitForFixedUpdate();
        
        // Now start raychecking 
        _doCheck = true;
    }

    protected void CheckCollision(Vector3 startPos, Vector3 endPos, bool reverse)
    {
        if (!_doCheck)
            return;
        
        // Define ray to use for raycast
        Ray ray = new Ray(startPos, endPos - startPos);
        
        // Limit Raycast to masks potentially used by Damage_Trigger's OnTriggerEnter
        var layerMask = 1 << 0 | 1 << 8 | 1 << 9 | 1 << 10 | 1 << 11 | (~0 << 16);
        
        // Exclude players and NPCs in the reverse direction to help prevent self-collisions
        if (reverse)
            layerMask &= ~LayerMask.GetMask("Player", "NPC");
        
        // Use Raycast with pre-allocated array to save on runtime memory allocation and GC
        var size = Physics.RaycastNonAlloc(ray, _raycastHits, Vector3.Distance(startPos, endPos), layerMask);
        
        // Process all collisions as if they just entered the trigger
        for (int i = 0; i < size; i++)
        {
            var collider = _raycastHits[i].collider;
            var originalPos = transform.position;
            transform.position = _raycastHits[i].point;
            if (!collider.isTrigger)
                _dt.OnTriggerEnter(_raycastHits[i].collider);

            if (gameObject != null)
                transform.position = originalPos;
        }
    }

    protected void CheckCollisions(Vector3 pos, Vector3 nextPos)
    {
        CheckCollision(pos, nextPos, false);
        CheckCollision(nextPos, pos, true);
    }

    protected void FixedUpdate()
    {
        var pos = _rb.position;
        var nextPos = pos + _rb.velocity * Time.fixedDeltaTime;
        
        // Do not bother checking if change between pos, nextPos is effectively 0
        if (Vector3.Distance(pos, nextPos) < Mathf.Epsilon)
            return;

        CheckCollisions(pos, nextPos);
    }
}