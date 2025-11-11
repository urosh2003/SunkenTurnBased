using System.Threading.Tasks;

public abstract class IStrategy
{
    public abstract Task ExecuteTurn(NpcAI npcAI);

    public abstract Task Act(NpcAI npcAI);
}