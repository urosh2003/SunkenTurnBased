public class SuperchargedPerk : Perk
{
    int damage;
    bool bonusReady;
    bool resetBuff;

    public SuperchargedPerk(Character character, int damage) : base(character)
    {
        this.damage = damage;
    }

    public override void Initialize()
    {
        owner.OnCharacterMove += RefreshSupercharge;
        owner.OnCharacterActionInitiated += AddDamage;
        owner.OnCharacterActed += CheckForResetBuff;
    }
    private void CheckForResetBuff(IAction action)
    {
        if (resetBuff)
        {
            bonusReady = false;
        }
        else
        {
            resetBuff = true;
        }
    }

    private void AddDamage(IAction action)
    {
        if (bonusReady)
        {
            action.bonusDamage += damage;
        }
    }

    private void RefreshSupercharge(int tilesMoved)
    {
        bonusReady = true;
        resetBuff = false;
    }


    public override void OnRemove()
    {
        owner.OnCharacterMove -= RefreshSupercharge;
        owner.OnCharacterActionInitiated -= AddDamage;
        owner.OnCharacterActed -= CheckForResetBuff;
    }
}