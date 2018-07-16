using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType  { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead };
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio };
public enum AITriggerEventType { Enter, Stay, Exit};
/// <summary>
/// Describe a potential target to the AI System.
/// </summary>
public struct AITarget
{
    //TODO make private and apply setters and getters
    private AITargetType _type;
    private Collider _collider;
    private Vector3 _position;
    private float _distance;
    private float _time;

    public AITargetType type { get { return _type; } }
    public Collider     collider { get { return _collider; } }
    public Vector3      position { get { return _position; } }      
    public float        distance { get { return _distance; } set { _distance = value; } }      
    public float        time { get { return _time; } }     

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _time = 0.0f;
    }
}

public abstract class AIStateMachine : MonoBehaviour
{
    // Public 
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    // Protected
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;


    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    [SerializeField] [Range(0, 20)] protected float _stoppingDistance = 1.0f;

    //Component Cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;


    // Public properties
    public Animator Animator { get { return _animator; } }
    public NavMeshAgent NavAgent { get { return _navAgent; } }
    public Vector3 sensorPosition
    {
        get
        {
            if(_sensorTrigger == null) { return Vector3.zero; }
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }
    public bool useRootPositon { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }

    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null) { return 0.0f; }
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
                                     _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }

    protected virtual void Awake()
    {
        // cache all components
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _transform = transform;

        if(GameSceneManager.instance != null)
        {
            // Register State machines with scene database
            if(_collider)
            {
                GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);   
            }
            if(_sensorTrigger)
            {
                GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
            }
        }
    }

    protected virtual void Start()
    {
        if(_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if(script != null)
            {
                script.parentStateMachine = this;
            }
        }

        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();
        foreach (AIState state in states)
        {
            if(state != null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state; // getting the state type is the key.
                state.SetStateMachine(this);
            }
        }

        if(_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }
    }
	
    protected virtual void  FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if(_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
    }

    /// <summary>
    /// called by unity each frame, Gives the current state a chance to 
    /// update itself.
    /// </summary>
    protected virtual void Update()
    {
        if (!_currentState) { return; }
        AIStateType newStateType = _currentState.OnUpdate();

        if(_currentStateType != newStateType)
        {
            AIState newState = null;
            // this functions (TryGetValue) first checks just like the containKey does
            // to see if this key exist in the dicenory and if it does
            // it will automatically populate the out parameter ( newState )
            // with the actual AIState Reference
            if(_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OneExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OneExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if(_targetTrigger)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        _target.Set(t, c, p, d);

        if (_targetTrigger)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        _target = t; 

        if (_targetTrigger)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void ClearTarget()
    {
        _target.Clear();
        if(_targetTrigger)
        {
            _targetTrigger.enabled = false;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) { return; }

        // Notify child state
        if(_currentState)
        {
            _currentState.OnDestinationReached(true);
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) { return; }

        // Notify child state
        if (_currentState)
        {
            _currentState.OnDestinationReached(false);
        }
    }

    /// <summary>
    /// This method is gonna be called by the Sensor Trigger 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="other"></param>
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if(_currentState)
        {
            _currentState.OnTriggerEvent(type, other);
        }
    }

    protected virtual void OnAnimatorMove()
    {
        if(_currentState)
        {
            _currentState.OnAnimatorUpdated();
        }
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if(_currentState)
        {
            _currentState.OnAnimatorIKUpdated();
        }
    }

    public void NavAgentControl (bool positionUpdate, bool rotationUpdate)
    {
        if(_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    /// <summary>
    /// Called by the state machine behaviours to enable/disable root motion
    /// </summary>
    /// <param name="rootPosition"></param>
    /// <param name="rootRotation"></param>

    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }
}
