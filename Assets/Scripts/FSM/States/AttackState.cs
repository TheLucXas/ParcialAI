using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private AttackData _data;
    private FSMAgent _agent;

    private Transform _target;
    private Agent _targetAgent;

    private float _rangedWaitTimer = 0f;

    public void SetTarget(Transform target) => _target = target;

    public AttackState(AttackData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }

    public override void Enter()
    {
        if (_target != null) _targetAgent = _target.GetComponentInParent<Agent>();

        _rangedWaitTimer = 0f;
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

        if (dist > _agent.PerceptionRadius + 1.5f)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        _agent.CurrentVelocity += SteeringUtils.CalculatePursuit(_agent.transform.position, _agent.CurrentVelocity, _data.ChaseSpeed, _data.ChaseForce, _target.position, _targetAgent.Velocity);

        _agent.CurrentVelocity = new Vector3(_agent.CurrentVelocity.x, 0f, _agent.CurrentVelocity.z);

        if (_agent.CurrentVelocity.sqrMagnitude > 0.0001f)
        {
            _agent.transform.position += _agent.CurrentVelocity * Time.deltaTime;
            _agent.transform.forward = _agent.CurrentVelocity.normalized;
            _agent.transform.position = Bounds.Instance.CalculateBoundPosition(_agent.transform.position);
        }

        if (dist <= _data.MeleeAttackRadius)
        {
            _targetAgent.TakeDamage(_data.AttackDamage * 2f);

            ShootCard(0f, true);

            _agent.AttackCooldownTimer = _data.TimeBetweenAttacks;
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }
        else if (dist <= _data.RangeAttackRadius)
        {
            _rangedWaitTimer += Time.deltaTime;

            if (_rangedWaitTimer >= _data.RangedAttackDelay)
            {
                ShootCard(_data.AttackDamage, false);

                _agent.AttackCooldownTimer = _data.TimeBetweenAttacks;
                _agent.FSM.ChangeState(_agent.Patrol);
                return;
            }
        }
        else
        {
            _rangedWaitTimer = 0f;
        }
    }

    private void ShootCard(float damage, bool isMelee)
    {
        if (_data.ProjectilePrefab != null)
        {
            Vector3 spawnPos = _agent.transform.position + (Vector3.up * 1f);
            Projectile newProjectile = Object.Instantiate(_data.ProjectilePrefab, spawnPos, Quaternion.identity);

            newProjectile.Initialize(_targetAgent, damage, isMelee, _data.MeleeAttackRadius);
        }
    }

    public override void Exit()
    {
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

    public float RangedAttackDelay = 1.5f;

    public Projectile ProjectilePrefab;
}