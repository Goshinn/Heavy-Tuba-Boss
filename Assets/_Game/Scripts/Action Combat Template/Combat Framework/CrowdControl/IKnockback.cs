public interface IKnockback
{
    bool IsKnockedback { get; }
    void AttemptKnockback(AttackInfo attackInfo, int thisCCDegree);
    void Knockback(AttackInfo attackInfo);
    void EndKnockback();
}
