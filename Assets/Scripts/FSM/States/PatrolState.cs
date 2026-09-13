using System.Collections.Generic;
using UnityEngine;

public class PatrolState : State
{
    private PatrolData _data;
    private int _currentIndex = 0;
    private float _timer;
    private float ballSpawnTime;
    private readonly FSMAgent _agent;
    public PatrolState(PatrolData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }

    public override void Enter()
    {
        _timer = 0f;
        ballSpawnTime = Random.Range(_data.minBallSpawnTime, _data.maxBallSpawnTime);
    }

    public override void Update()
    {
        _timer += Time.deltaTime/60;
        Transform nextWaypoint = _data.waypoints[_currentIndex];
        if (Vector3.Distance(nextWaypoint.position, _data.transform.position) <= _data.waypointCheckDistance)
        {
            _currentIndex = _currentIndex + 1 < _data.waypoints.Count ? _currentIndex + 1 : 0;
            nextWaypoint = _data.waypoints[_currentIndex];
        }

        Vector3 dir = nextWaypoint.position - _data.transform.position;

        _data.transform.position += _data.speed * Time.deltaTime * dir.normalized;
        _data.transform.forward = dir;

        if (_timer >= ballSpawnTime)
        {
            //_agent.FSM.ChangeState(_agent.SpawnBall);
            Instantiate(_data.ball, _data.transform.position, _data.transform.rotation);
            ballSpawnTime = Random.Range(_data.minBallSpawnTime, _data.maxBallSpawnTime);
        }
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
    public float minBallSpawnTime = 3f;
    public float maxBallSpawnTime = 9f;
}
