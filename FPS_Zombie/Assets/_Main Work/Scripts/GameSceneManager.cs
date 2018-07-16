using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    // Statics
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameSceneManager>();
            }
            return _instance;
        }
    }
    // Private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

    // Public Methods

    /// <summary>
    /// Stores the passed state machine in the dictionary with the supplied key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="stateMachine"></param>
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if(!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    /// <summary>
    /// returns an AI State machine reference searched by 
    /// the instance ID of an object
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if(_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }
}
