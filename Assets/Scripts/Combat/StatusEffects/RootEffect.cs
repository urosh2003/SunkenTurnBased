public class RootEffect : StatusEffect
{
    public RootEffect(int duration, Character character) : base(duration, character)
    {
        this.name = StatusEffectName.ROOT;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantMove;
        owner.OnCharacterEndTurn += DecreaseDuration;
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantMove;
        owner.OnCharacterEndTurn -= DecreaseDuration;
    }
}