using System.Threading.Tasks;
using UnityEngine;

public abstract class IPlayerState
{
    public IAction selectedAction;
    public abstract void Update(Vector3 mouseWorldPosition);

    public abstract void Enter();
    public abstract void Exit();

    public abstract Task<bool> Execute();
}