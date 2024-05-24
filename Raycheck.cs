using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace LunacidQoL;

public class Raycheck : MonoBehaviour
{
    public ManualLogSource Logger;
    
    private Rigidbody _rb;
    private Damage_Trigger _dt;
    private MethodInfo _dtTriggerEnter;
    private bool _doCheck;

    private RaycastHit[] _raycastHits = new RaycastHit[8];
    
    protected void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        _dt = GetComponent<Damage_Trigger>();
        _dtTriggerEnter = typeof(Damage_Trigger).GetMethod("OnTriggerEnter", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        if (_dtTriggerEnter == null)
            Logger.LogError("Could not access OnTriggerEnter of Damage_Trigger");

        if (_dt != null && _dtTriggerEnter != null && !_dt.Constant)
        {
            var timer = typeof(Damage_Trigger).GetField("last", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            timer.SetValue(_dt, Time.time - Mathf.Epsilon);
            
            _doCheck = true;
        }
            
    }

    protected void CheckCollision(Vector3 startPos, Vector3 endPos)
    {
        Ray ray = new Ray(startPos, endPos - startPos);
        
        // Limit Raycast to masks potentially used by Damage_Trigger's OnTriggerEnter
        var layerMask = 1 << 0 | 1 << 8 | 1 << 9 | 1 << 10 | 1 << 11 | (~0 << 16);
        
        var size = Physics.RaycastNonAlloc(ray, _raycastHits, Vector3.Distance(startPos, endPos), layerMask);

        if (_doCheck)
        {
            for (int i = 0; i < size; i++)
                _dtTriggerEnter.Invoke(_dt, new object[] { _raycastHits[i].collider });
        }
    }

    protected void FixedUpdate()
    {
        var pos = _rb.position;
        var nextPos = pos + _rb.velocity * Time.fixedDeltaTime;
        
        // Do not bother checking if change between pos, nextPos is effectively 0
        if (Vector3.Distance(pos, nextPos) < Mathf.Epsilon)
            return;
        
        CheckCollision(pos, nextPos);
        CheckCollision(nextPos, pos);
    }
}