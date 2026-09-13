using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PatrolState : State
{
    private PatrolData _data;
    private int _currentIndex = 0;
    private float _timer;
    private float _ballSpawnTime;
    private int _ballsCount = 0;
    private readonly FSMAgent _agent;
    private readonly Collider[] _detectionBuffer = new Collider[1];
    public PatrolState(PatrolData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }

    public override void Enter()
    {
        _timer = 0f;
        _ballSpawnTime = Random.Range(_data.minBallSpawnTime, _data.maxBallSpawnTime);
    }

    public override void Update()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(_data.transform.position, _data.detectionRadius, _detectionBuffer, _data.boidLayer);

        if (hitCount > 0 && _agent.AttackCooldownTimer <= 0f)
        {
            _agent.Attack.SetTarget(_detectionBuffer[0].transform);
            _agent.FSM.ChangeState(_agent.Attack);
            return;
        }

        _timer += Time.deltaTime;
        Transform nextWaypoint = _data.waypoints[_currentIndex];

        Vector3 agentPos2D = new Vector3(_data.transform.position.x, 0f, _data.transform.position.z);
        Vector3 waypointPos2D = new Vector3(nextWaypoint.position.x, 0f, nextWaypoint.position.z);

        if (Vector3.Distance(waypointPos2D, agentPos2D) <= _data.waypointCheckDistance)
        {
            _currentIndex = _currentIndex + 1 < _data.waypoints.Count ? _currentIndex + 1 : 0;
            nextWaypoint = _data.waypoints[_currentIndex];

            waypointPos2D = new Vector3(nextWaypoint.position.x, 0f, nextWaypoint.position.z);
        }

        _agent.CurrentVelocity += SteeringUtils.CalculateSeek(agentPos2D, _agent.CurrentVelocity, waypointPos2D, _data.speed, _data.steerForce);
        _agent.CurrentVelocity = new Vector3(_agent.CurrentVelocity.x, 0f, _agent.CurrentVelocity.z);

        _data.transform.position += _agent.CurrentVelocity * Time.deltaTime;

        if (_agent.CurrentVelocity.sqrMagnitude > 0.0001f)
        {
            _data.transform.forward = _agent.CurrentVelocity.normalized;
        }

        if (_timer >= _ballSpawnTime && _ballsCount + 1 <= _data.maxBallsCount)
        {
            var pos = new Vector3(_data.transform.position.x, 0f, _data.transform.position.z);
            GameObject ballObj = Object.Instantiate(_data.ball, pos, _data.transform.rotation);
            _ballsCount++;
            if (ballObj.TryGetComponent<Ball>(out var ballEntity)) ballEntity.OnDestroyed += SubtractBall;
                _ballSpawnTime = Random.Range(_data.minBallSpawnTime, _data.maxBallSpawnTime);
            _timer = 0;
        }

    }

    private void SubtractBall(Ball ball)
    {
        ball.OnDestroyed -= SubtractBall;
        _ballsCount--;
    }

    public override void Exit()
    {
    }
}

[System.Serializable]
public class PatrolData
{
    public List<Transform> waypoints;
    public Transform transform;
    public GameObject ball;
    public float waypointCheckDistance;
    public float speed = 13f;
    public float steerForce = 10f;
    public float detectionRadius = 8f;
    public float minBallSpawnTime = 3f;
    public float maxBallSpawnTime = 9f;
    public float maxBallsCount = 5;
    public LayerMask boidLayer;
}
