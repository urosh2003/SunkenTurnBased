
public enum StatusEffectName { STUN, ROOT, CHAINED, DISARMED, CURSE, DOT, HOT, ENGINEOFF };
public enum StatusEffectType { BUFF, DEBUFF };
public abstract class StatusEffect
{
    public StatusEffectName name;
    public StatusEffectType type;
    public int duration;
    public Character owner;
    public Character relatedCharacter;

    public StatusEffect(int duration, Character character, Character relatedCharacter = null)
    {
        this.duration = duration;
        this.owner = character;
        this.relatedCharacter = relatedCharacter;
        Initialize();
    }

    public void DecreaseDuration()
    {
        this.duration -= 1;
        if (this.duration <= 0)
        {
            Remove();
        }
    }

    public abstract void Initialize();

    public abstract void OnRemove();

    public void Remove()
    {
        OnRemove();
        owner.activeEffects.Remove(this);
    }

}

