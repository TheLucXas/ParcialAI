using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private AttackData _data;

    private float MeleeAttackRadius = 3f;
    private float RangeAttackRadius = 6f;

    public AttackState(AttackData data)
    {
        _data = data;
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
    }
}

[System.Serializable]
public class AttackData
{
    public Transform target;
}