using System.Threading.Tasks;
using UnityEngine;

public abstract class IState
{
    public abstract void Update(Vector3 mouseWorldPosition);

    public abstract void Enter();
    public abstract void Exit();

    public abstract Task<bool> Execute();


}