public class GainMomentumPerk : Perk
{
    int currentMomentum;
    int lastMomentum;
    bool bonusReady;
    bool resetBuff;

    public GainMomentumPerk(Character character) : base(character)
    {
    }

    public override void Initialize()
    {
        owner.OnCharacterMove += GainMomentum;
        owner.OnCharacterActionInitiated += AddDamage;
        owner.OnCharacterActed += CheckForResetBuff;
    }
    private void CheckForResetBuff(IAction action)
    {
        if (lastMomentum == currentMomentum)
        {
            currentMomentum = 0;
            lastMomentum = 0;
        }
        else
        {
            lastMomentum = currentMomentum;
        }
    }

    private void AddDamage(IAction action)
    {

        action.bonusDamage += currentMomentum;

    }

    private void GainMomentum(int tilesMoved)
    {
        lastMomentum = currentMomentum;
        currentMomentum += tilesMoved;
    }


    public override void OnRemove()
    {
        owner.OnCharacterMove -= GainMomentum;
        owner.OnCharacterActionInitiated -= AddDamage;
        owner.OnCharacterActed -= CheckForResetBuff;


    }
}