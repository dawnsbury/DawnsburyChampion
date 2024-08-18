using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Champion;

namespace ChampionFocusSpells;

public class ChampionSpells
{
    public static SpellId LayOnHandsId;
    public static SpellId SunBladeId;

    public static void LoadSpells()
    {
        RegisterLayOnHands();
        RegisterSunBlade();
    }

    public static void RegisterLayOnHands()
    {
        LayOnHandsId = ModManager.RegisterNewSpell("Lay on Hands", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) =>
         {
             var flavorText = "Your hands become infused with positive energy, healing a living creature or damaging an undead creature with a touch.";
             var rulesText = "If you use Lay on Hands on a willing living target, you restore " + S.HeightenedVariable(spellLevel * 6, 1) + " Hit Points; if the target is one of your allies, they also gain a {b}+2 status bonus to AC{/b} for 1 round." +
             "\n\nAgainst an undead target, you deal {b}" + S.HeightenedVariable(spellLevel, 1) + "d6{/b} damage and it must attempt a {b}basic Fortitude save{/b}; if it fails, it also takes a {b}–2 status penalty to AC{/b} for 1 round.";

             CombatAction spell = Spells.CreateModern(IllustrationName.HealersGloves, "Lay on Hands", new[] { Trait.Uncommon, DawnsburyChampion.ChampionTrait, Trait.Healing, Trait.Necromancy, Trait.Positive, Trait.Manipulate, Trait.Focus },
                    flavorText,
                     rulesText,
                     Target.AdjacentCreatureOrSelf((Target t, Creature a, Creature d) => a.AI.Heal(d, 4.5f)).WithAdditionalConditionOnTargetCreature((a, d) => EffectiveTargetingCondition(a, d)),
                     spellLevel,
                     SpellSavingThrow.Basic(Defense.Fortitude))
                 .WithSoundEffect(Dawnsbury.Audio.SfxName.Healing)
                 .WithEffectOnEachTarget((async (spell, caster, target, result) =>
                 {
                     if (target.HasTrait(Trait.Undead))
                     {
                         DiceFormula damageFormula = DiceFormula.FromText(spellLevel + "d6");
                         await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, damageFormula, DamageKind.Positive);

                         if (result == CheckResult.Failure || result == CheckResult.CriticalFailure)
                         {
                             target.AddQEffect(new QEffect("Lay on Hands", "You have a -2 status penalty to AC.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, spellcaster, IllustrationName.HealersGloves)
                             {
                                 CountsAsADebuff = true,
                                 BonusToDefenses = (QEffect _, CombatAction? _, Defense defense) =>
                                 {
                                     if (defense == Defense.AC)
                                     {
                                         return new Bonus(-2, BonusType.Status, "Lay on Hands");
                                     }
                                     return null;
                                 }
                                 ,
                             }
                                 );
                         };
                     }
                     else
                     {
                         string effectDescription = "You gain a {b}+2 status bonus to AC{/b}";
                         if (spellcaster.HasFeat(DawnsburyChampionFeats.AcceleratingTouchFeatName))
                         {
                             effectDescription = effectDescription += " and a {b}+10 status bonus to speed{/b}";
                         }
                         target.Heal((spellLevel * 6).ToString(), spell);
                         QEffect layOnHandsBuff = new QEffect("Lay on Hands", effectDescription + ".", ExpirationCondition.ExpiresAtStartOfSourcesTurn, spellcaster, IllustrationName.HealersGloves)
                         {
                             CountsAsABuff = true,
                             BonusToDefenses = (QEffect _, CombatAction? _, Defense defense) =>
                             {
                                 if (defense == Defense.AC)
                                 {
                                     return new Bonus(2, BonusType.Status, "Lay on Hands");
                                 }
                                 return null;
                             }
                         };
                         if (spellcaster.HasFeat(DawnsburyChampionFeats.AcceleratingTouchFeatName))
                         {
                             layOnHandsBuff.BonusToAllSpeeds = (QEffect _) => new Bonus(2, BonusType.Status, "Accelerating Touch");
                         }
                         target.AddQEffect(layOnHandsBuff);
                     }
                 })).WithActionCost(1).WithNoSaveFor((CombatAction spell, Creature target) => WouldHeal(target));

             return spell;
         });

        bool WouldHeal(Creature target)
        {
            if (target.HasTrait(Trait.Undead))
            {
                return false;
            }
            return true;
        }

        Usability EffectiveTargetingCondition(Creature caster, Creature target)
        {
            if ((target.EnemyOf(caster) && target.HasTrait(Trait.Undead)) || (target.FriendOf(caster) && !target.HasTrait(Trait.Undead)))
            {
                return Usability.Usable;
            }
            return Usability.CommonReasons.TargetIsNotPossibleForComplexReason;
        }

    }
    public static void RegisterSunBlade()
    {
        SunBladeId = ModManager.RegisterNewSpell("SunBlade", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) =>
        {
            var flavorText = "You fire a ray of burning sunlight from your weapon.";
            var rulesText = "You must be wielding a sword or spear to cast sun blade, and you perform this spell’s somatic component with the weapon. Make a spell attack roll. The ray deals {b}" + (spellLevel - 1).ToString() + "d4 fire{/b} damage." +
            "If the target is evil, the ray deals an additional {b}" + (spellLevel - 1).ToString() + "d4 good{/b} damage, and if the target is undead, the ray deals an additional{b}" + (spellLevel - 1).ToString() + "d4 positive{/b} damage (both effects apply against creatures that are both evil and undead).";
            CombatAction spell = Spells.CreateModern(IllustrationName.MagicWeapon, "Sun Blade", new[] { Trait.Uncommon, DawnsburyChampion.ChampionTrait, Trait.Evocation, Trait.Fire, Trait.Light, Trait.Positive, Trait.Manipulate, Trait.Focus },
                   flavorText,
                    rulesText,
                    Target.Ranged(6).WithAdditionalConditionOnTargetCreature((Creature spellcaster, Creature defender) =>
                    {
                        List<Item> items = spellcaster.HeldItems;
                        bool holdingValidWeapon = false;
                        foreach (Item item in items)
                        {
                            if (item.HasTrait(Trait.Sword) || item.HasTrait(Trait.Spear))
                            {
                                holdingValidWeapon = true;
                                break;
                            }
                        }
                        if (!holdingValidWeapon)
                        {
                            return Usability.NotUsable(("You must be holding a sword or spear to cast the spell."));
                        }
                        return Usability.Usable;
                    }),
                    spellLevel, null)
                .WithSpellAttackRoll()
                .WithSoundEffect(Dawnsbury.Audio.SfxName.FireRay)
                .WithEffectOnEachTarget((async (spell, caster, target, result) =>
                {
                    DiceFormula damage = DiceFormula.FromText((spellLevel - 1).ToString() + "d4");
                    KindedDamage[] damages = new KindedDamage[] { new KindedDamage(damage, DamageKind.Fire) };

                    if (target.HasTrait(Trait.Evil))
                    {
                        damages = damages.Append(new KindedDamage(damage, DamageKind.Good)).ToArray();
                        QEffect bonusDamage = new QEffect(ExpirationCondition.Ephemeral);
                    }

                    if (target.HasTrait(Trait.Undead))
                    {
                        damages = damages.Append(new KindedDamage(damage, DamageKind.Positive)).ToArray();
                    }
                    await CommonSpellEffects.DealAttackRollDamage(spell, caster, target, result, damages);
                })).WithActionCost(2);
            return spell;
        });
    }
}