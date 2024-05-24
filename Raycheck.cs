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
    
    protected void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        _dt = GetComponent<Damage_Trigger>();
        _dtTriggerEnter = typeof(Damage_Trigger).GetMethod("OnTriggerEnter", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        if (_dtTriggerEnter == null)
            Logger.LogError("Could not access OnTriggerEnter of Damage_Trigger");

        if (_dt != null && _dtTriggerEnter != null)
        {
            var timer = typeof(Damage_Trigger).GetField("last", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            timer.SetValue(_dt, Time.time - Mathf.Epsilon);
            
            _doCheck = !_dt.Constant;
        }
            
    }

    protected void CheckCollision(Vector3 startPos, Vector3 endPos)
    {
        Ray ray = new Ray(startPos, endPos - startPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, Vector3.Distance(startPos, endPos), Physics.AllLayers);

        if (_doCheck)
        {
            foreach (var hit in hits)
                _dtTriggerEnter.Invoke(_dt, new object[] { hit.collider });
        }
    }

    protected void FixedUpdate()
    {
        var pos = _rb.position;
        var nextPos = pos + _rb.velocity * Time.fixedDeltaTime;
        CheckCollision(pos, nextPos);
        CheckCollision(nextPos, pos);
    }
}