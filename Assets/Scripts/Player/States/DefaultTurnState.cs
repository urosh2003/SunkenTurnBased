
using System.Threading.Tasks;
using UnityEngine;

public class DefaultTurnState : IState
{
    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override async Task<bool> Execute()
    {
        return true;
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void Update(Vector3 mousePosition)
    {

    }
}