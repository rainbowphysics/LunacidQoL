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
            _doCheck = true;
        }
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
            if (!collider.isTrigger)
                _dt.OnTriggerEnter(_raycastHits[i].collider);
        }
    }

    protected void CheckCollisions(Vector3 pos, Vector3 nextPos)
    {
        CheckCollision(pos, nextPos, false);
        CheckCollision(nextPos, pos, true);
    }

    protected void FixedUpdate()
    {
        // Difference in backwards and forward directions
        var dx = _rb.velocity * Time.fixedDeltaTime / 4;
        // Do not bother checking if dx is negligible 
        if (Vector3.SqrMagnitude(dx) < Mathf.Epsilon)
            return;
        
        // Get current position, then extrapolate backwards and forward 
        var curPos = _rb.position;
        var prevPos = curPos - 3 * dx;
        var nextPos = curPos + dx;
        CheckCollisions(prevPos, nextPos);
    }
}