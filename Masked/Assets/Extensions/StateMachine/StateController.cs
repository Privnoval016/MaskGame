using System.Collections.Generic;
using UnityEngine;

namespace Extensions.StateMachine
{
    public class StateController<T> : MonoBehaviourUpdatable where T : State
    {
        Stack<T> currentState = new Stack<T>();
        public MonoBehaviour parent;

        public StateController(MonoBehaviour parent) : base(parent.gameObject)
        {
            this.parent = parent;
        }

        public override void Update()
        {
            if (currentState.Count > 0)
            {
                currentState.Peek().OnUpdate();
            }
        }

        public override void FixedUpdate()
        {
            if (currentState.Count > 0)
            {
                currentState.Peek().OnFixedUpdate();
            }
        }

        public override void LateUpdate()
        {
            if (currentState.Count > 0)
            {
                currentState.Peek().OnLateUpdate();
            }
        }

        public void ChangeState(T newState)
        {
            RemoveTop();
            AddNewState(newState);
        }

        public void Interrupt(T newState)
        {
            currentState.Peek().OnInterrupt();
            AddNewState(newState);
        }

        public void ResumePrevious()
        {
            RemoveTop();
            if (currentState.Count > 0)
            {
                currentState.Peek().OnResume();
            }
        }

        public State GetCurrentState()
        {
            return currentState != null && currentState.Count > 0 ? currentState.Peek() : null;
        }
        
        public bool IsState<TState>() where TState : State
        {
            return currentState.Count > 0 && currentState.Peek() is TState;
        }

        private void RemoveTop()
        {
            if (currentState.Count > 0 && !currentState.Peek().doNotRemove)
            {
                currentState.Peek().OnExit();
                currentState.Pop();
            }
        }

        private void AddNewState(T newState)
        {
            currentState.Push(newState);
            currentState.Peek().OnStateEnter(parent);
        }

        public void ClearStates()
        {
            while (currentState.Count > 0 && !currentState.Peek().doNotRemove)
            {
                currentState.Pop();
            }
        }

        public void PrintStates()
        {
            string output = parent.name + " States: ";

            foreach (T state in currentState.ToArray())
            {
                output += state.ToString();
                output += " -- ";
            }

            Debug.Log(output[..^4]);


        }

        public override void OnTriggerEnter(Collider other)
        {
            if (currentState.Count > 0)
            {
                currentState.Peek().OnTriggerEnter(other);
            }
        }

        public override void OnCollisionEnter(Collision collision)
        {
            if (currentState.Count > 0)
            {
                currentState.Peek().OnCollisionEnter(collision);
            }
        }
    }
}

