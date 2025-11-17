public class DisarmEffect : StatusEffect
{
    public DisarmEffect(int duration, Character character) : base(duration, character)
    {
        this.name = StatusEffectName.DISARMED;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantAttack;
        owner.OnCharacterEndTurn += DecreaseDuration;
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantAttack;
        owner.OnCharacterEndTurn -= DecreaseDuration;
    }
}