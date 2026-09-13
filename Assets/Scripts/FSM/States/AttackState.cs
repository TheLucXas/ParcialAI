using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private AttackData _data;
    private FSMAgent _agent;

    private bool _isAttacking = false;
    private Transform _target;
    private Agent _targetAgent;

    public void SetTarget(Transform target) => _target = target;

    public AttackState(AttackData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }

    public override void Enter()
    {
        _isAttacking = false;

        if (_target != null) _targetAgent = _target.GetComponentInParent<Agent>();

    }

    public override void Update()
    {
        if (_targetAgent != null && _targetAgent.IsDead)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        if (_target == null)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float dist = Vector3.Distance(_target.position, _agent.transform.position);

        if (dist > _agent.PerceptionRadius)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        _agent.CurrentVelocity += SteeringUtils.CalculatePursuit(_agent.transform.position, _agent.CurrentVelocity, _data.ChaseSpeed, _data.ChaseForce, _target.position, _targetAgent.Velocity);
        if (_agent.CurrentVelocity.sqrMagnitude > 0.0001f)
        {
            _agent.transform.position += _agent.CurrentVelocity * Time.deltaTime;
            _agent.transform.forward = _agent.CurrentVelocity;
            _agent.transform.position = Bounds.Instance.CalculateBoundPosition(_agent.transform.position);
        }

        if (!_isAttacking && dist <= _data.MeleeAttackRadius)
        {
            _targetAgent.TakeDamage(_data.AttackDamage * 2f);

            _agent.AttackCooldownTimer = _data.TimeBetweenAttacks;

            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }
        else if (!_isAttacking && dist <= _data.RangeAttackRadius)
        {
            _targetAgent.TakeDamage(_data.AttackDamage);

            _agent.AttackCooldownTimer = _data.TimeBetweenAttacks;

            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }
    }

    public override void Exit()
    {
        _isAttacking = false;
        _target = null;
        _targetAgent = null;
    }
}

[System.Serializable]
public class AttackData
{
    public float AttackDamage = 1f;
    public float MeleeAttackRadius = 3f;
    public float RangeAttackRadius = 6f;
    public float TimeBetweenAttacks = 5f;
    public float ChaseSpeed = 14f;
    public float ChaseForce = 14f;
}