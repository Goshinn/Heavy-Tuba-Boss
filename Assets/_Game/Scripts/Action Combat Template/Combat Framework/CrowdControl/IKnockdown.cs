public interface IKnockdown
{
    bool IsKnockeddown { get; }
    void AttemptKnockdown(AttackInfo attackInfo, int thisCCDegree);
    void Knockdown(AttackInfo attackInfo);
    void EndKnockdown();
}
