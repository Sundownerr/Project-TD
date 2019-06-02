
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update() => CurrentState?.Execute();
    
}

