using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics.Enumerations;

public class ChampionsReactionModification : DamageModification
{
    private readonly int reductionAmount;

    private readonly string explanation;

    private readonly List<Resistance> appliedResistances;

    public ChampionsReactionModification(int amount, string explanation, List<Resistance> resistances)
    {
        reductionAmount = amount;
        this.explanation = explanation;
        appliedResistances = resistances;
    }

    public override void Apply(DamageEvent damageEvent)
    {
        int totalReduction = 0;
        int appliedReduction = reductionAmount;

        foreach (KindedDamage kindedDamage in damageEvent.KindedDamages)
        {
            DamageKind damageKind = kindedDamage.DamageKind;

            foreach (Resistance resistance in appliedResistances)
            {
                if (resistance.DamageKind == damageKind)
                {
                    if (resistance.Value >= appliedReduction)
                    {
                        appliedReduction = resistance.Value;
                    }
                    else
                    {
                        appliedReduction = reductionAmount - resistance.Value;
                    }
                }
            }

            if (kindedDamage.ResolvedDamage < appliedReduction)
            {
                int difference = Math.Abs(kindedDamage.ResolvedDamage - reductionAmount);
                kindedDamage.ResolvedDamage = 0;
                totalReduction += difference;
            }
            else
            {
                kindedDamage.ResolvedDamage -= appliedReduction;
                totalReduction += appliedReduction;
            }
        }

        if (totalReduction != 0)
        {
            damageEvent.DamageEventDescription.AppendLine("{b}-" + totalReduction + "{/b} " + explanation);
        }
    }
}