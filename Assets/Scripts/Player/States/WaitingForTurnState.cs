using System.Threading.Tasks;
using UnityEngine;

public class WaitingForTurnState : IState
{
    public override void Enter()
    {

    }

    public override async Task<bool> Execute()
    {
        return false;
    }

    public override void Exit()
    {

    }

    public override void Update(Vector3 mousePosition)
    {
        
    }
}