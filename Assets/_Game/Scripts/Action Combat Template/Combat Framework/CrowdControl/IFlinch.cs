public interface IFlinch
{
    bool IsFlinched { get; }
    void AttemptFlinch(AttackInfo attackInfo, int thisCCDegree);
    void Flinch(AttackInfo attackInfo);
    void EndFlinch();
}