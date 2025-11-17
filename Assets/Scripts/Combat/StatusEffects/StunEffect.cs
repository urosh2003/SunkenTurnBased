public class StunEffect : StatusEffect
{
    public StunEffect(int duration, Character character) : base(duration, character)
    {
        this.name = StatusEffectName.STUN;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantAct;
        owner.OnCharacterEndTurn += DecreaseDuration;
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantAct;
        owner.OnCharacterEndTurn -= DecreaseDuration;
    }
}