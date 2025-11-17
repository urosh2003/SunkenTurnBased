using System.Threading.Tasks;

public abstract class IStrategy
{
    public virtual async Task ExecuteTurn(NpcAI npcAI) { }

    public virtual async Task Act(NpcAI npcAI) { }
}