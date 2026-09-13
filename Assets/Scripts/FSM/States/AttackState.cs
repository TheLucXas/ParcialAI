using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private AttackData _data;
    private FSMAgent _agent;

    public AttackState(AttackData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
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
    public float MeleeAttackRadius = 3f;
    public float RangeAttackRadius = 6f;
}