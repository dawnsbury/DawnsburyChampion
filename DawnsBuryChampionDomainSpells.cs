using Dawnsbury.Audio;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Mods.Classes.Champion;

namespace ChampionDomainSpells
{
    public static class ChampionDomainSpells
    {
        public static CombatAction? LoadModernSpell(SpellId spellId, Creature? owner, int level, bool inCombat, SpellInformation spellInformation)
        {
            switch (spellId)
            {
                case SpellId.FireRay:
                    {
                        string success = "Deal " + S.HeightenedVariable(level * 2, 2) + "d6 fire damage.";
                        string criticalSuccess = "Double damage, and " + S.HeightenedVariable(level, 1) + "d4 persistent fire damage.";
                        return Spells.CreateModern(IllustrationName.FireRay, "Fire Ray", new Trait[5]
                        {
                    Trait.Attack,
                    DawnsburyChampion.ChampionTrait,
                    Trait.Evocation,
                    Trait.Fire,
                    Trait.Focus
                        }, "A blazing band of fire arcs through the air.", "Make a spell attack roll." + S.FourDegreesOfSuccessReverse(null, null, success, criticalSuccess) + S.HeightenText(level, 1, inCombat, "{b}Heightened (+1){/b} The ray's initial damage increases by 2d6, and the persistent fire damage on a critical success increases by 1d4."), Target.Ranged(12), level, null).WithSpellAttackRoll().WithSoundEffect(SfxName.FireRay)
                            .WithProjectileCone(IllustrationName.FireRay, 15, ProjectileKind.Ray)
                            .WithEffectOnEachTarget(async delegate (CombatAction action, Creature caster, Creature target, CheckResult checkResult)
                            {
                                await CommonSpellEffects.DealAttackRollDamage(action, caster, target, checkResult, 2 * level + "d6", DamageKind.Fire);
                                if (checkResult == CheckResult.CriticalSuccess)
                                {
                                    target.AddQEffect(QEffect.PersistentDamage(level + "d4", DamageKind.Fire));
                                }
                            });
                    }
                case SpellId.AgileFeet:
                    return Spells.CreateModern(IllustrationName.WarpStep, "Agile Feet", new Trait[3]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Transmutation,
                    Trait.Focus
                    }, "The blessings of your god make your feet faster and your movements more fluid.", "Until end of turn, you gain a +5-foot status bonus to your Speed and ignore difficult terrain.\n\nStride or Step.", Target.Self(), level, null).WithActionCost(1).WithSoundEffect(SfxName.Footsteps)
                        .WithEffectOnSelf(async delegate (CombatAction action, Creature self)
                        {
                            QEffect qfAgileFeet = new QEffect("Agile Feet", "You have a +5 to Speed and you ignore difficult terrain.", ExpirationCondition.ExpiresAtEndOfAnyTurn, self, (Illustration)IllustrationName.WarpStep)
                            {
                                CountsAsABuff = true,
                                BonusToAllSpeeds = (QEffect _) => new Bonus(1, BonusType.Status, "Agile Feet"),
                                Id = QEffectId.IgnoresDifficultTerrain
                            };
                            self.AddQEffect(qfAgileFeet);
                            if (!(await self.StrideAsync("Choose where to Step or Stride with Agile Feet.", allowStep: true, maximumFiveFeet: false, null, allowCancel: true)))
                            {
                                self.RemoveAllQEffects((QEffect qf) => qf == qfAgileFeet);
                                action.RevertRequested = true;
                            }
                        });
                case SpellId.WinterBolt:
                    return Spells.CreateModern(IllustrationName.WinterBolt, "Winter Bolt", new Trait[5]
                    {
                    Trait.Attack,
                    Trait.Cold,
                    Trait.Evocation,
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus
                    }, "You fling a hollow icicle filled with winter's wrath.", "Make a spell attack roll. The bolt deals " + S.HeightenedVariable(level, 1) + "d8 piercing damage and lodges in the target. At the end of the target's next turn, the bolt shatters, releasing a whirl of snow and ice that deals " + S.HeightenedVariable(level, 1) + "d12 cold damage to the target and all adjacent creatures. The bolt can be removed with an Interact action, which causes it to melt harmlessly without detonating. {i}(Mindless creatures can't remove the bolt.){/i}" + S.FourDegreesOfSuccessReverse(null, null, "Deal full damage.", "Deal full damage, and the bolt is especially well-anchored, taking 2 actions to remove, and the bolt's explosion deals double damage.") + S.HeightenText(level, 1, inCombat, "{b}Heightened (+1){/b} The initial damage increases by 1d8 and the secondary damage increases by 1d12."), Target.Ranged(12), level, null).WithSpellAttackRoll().WithSoundEffect(SfxName.WinterBolt)
                        .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult checkResult)
                        {
                            CombatAction spell2 = spell;
                            Creature caster3 = caster;
                            if (checkResult >= CheckResult.Success)
                            {
                                DiceFormula damage = DiceFormula.FromText(level + "d8", spell2.Name);
                                await caster3.DealDirectDamage(spell2, damage, target, checkResult, DamageKind.Piercing);
                                QEffect qfLodgedBolt = new QEffect("Lodged {i}winter bolt{/i}", "At end of your turn, the bolt will explode and deal heavy cold damage to you and all adjacent creatures unless you pull it out first.", ExpirationCondition.Never, caster3, (Illustration)IllustrationName.WinterBolt)
                                {
                                    CountsAsADebuff = true,
                                    EndOfYourTurn = async delegate (QEffect qfSelf, Creature you)
                                    {
                                        IEnumerable<Creature> enumerable = you.Occupies.Neighbours.Creatures.Append(you);
                                        foreach (Creature item in enumerable)
                                        {
                                            await CommonSpellEffects.DealAttackRollDamage(spell2, caster3, item, checkResult, level + "d12", DamageKind.Cold);
                                            qfSelf.ExpiresAt = ExpirationCondition.Immediately;
                                        }
                                    }
                                };
                                qfLodgedBolt.ProvideContextualAction = (QEffect qfSelf) => new ActionPossibility(new CombatAction(qfSelf.Owner, IllustrationName.WinterBolt, "Remove lodged {i}winter bolt{/i}", new Trait[1] { Trait.Manipulate }, "Remove the bolt that's lodged in your body. If you don't do this before the end of your turn, it will explode, dealing heavy damage to you and all adjacent creatures.", Target.Self((Creature self, AI ai) => ai.AlwaysIfSmartAndTakingCareOfSelf)).WithActionCost((checkResult != CheckResult.CriticalSuccess) ? 1 : 2).WithEffectOnSelf(delegate (Creature self)
                                {
                                    self.RemoveAllQEffects((QEffect qf) => qf == qfLodgedBolt);
                                })).WithPossibilityGroup("Remove debuff");
                                target.AddQEffect(qfLodgedBolt);
                            }
                        });
                case SpellId.BitOfLuck:
                    return Spells.CreateModern(IllustrationName.BitOfLuck, "Bit of Luck", new Trait[4]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Fortune,
                    Trait.Divination
                    }, "You tilt the scales of luck slightly to protect a creature from disaster.", "When the target would attempt a saving throw this encounter, it can roll twice and use the better result. Once it does this, the spell ends.\n\nIf you cast {i}bit of luck{/i} again, any previous {i}bit of luck{/i} you cast that's still in effect ends. After a creature has been targeted with {i}bit of luck{/i}, it becomes temporarily immune for the rest of the day.", Target.RangedFriend(6).WithAdditionalConditionOnTargetCreature((Creature a, Creature d) => d.PersistentUsedUpResources.UsedBitOfLuck ? Usability.CommonReasons.UsedDailyPowerToday : Usability.Usable), level, null).WithSoundEffect(SfxName.BitOfLuck).WithEffectOnEachTarget(async delegate (CombatAction spell, Creature self, Creature target, CheckResult result)
                    {
                        Creature self2 = self;
                        self2.Battle.AllCreatures.ForEach(delegate (Creature cr)
                        {
                            cr.RemoveAllQEffects((QEffect qf) => qf.Id == QEffectId.BitOfLuck && qf.Source == self2);
                        });
                        target.PersistentUsedUpResources.UsedBitOfLuck = true;
                        QEffect qEffect = new QEffect("Bit of Luck", "Once this encounter, when you make a saving throw, you can roll twice and use the better result.", ExpirationCondition.Never, self2, (Illustration)IllustrationName.BitOfLuck)
                        {
                            CountsAsABuff = true,
                            Id = QEffectId.BitOfLuck,
                            BeforeYourSavingThrow = CommonSpellEffects.BitOfLuck(IllustrationName.BitOfLuck, "Bit of Luck")
                        };
                        target.AddQEffect(qEffect);
                    });
                case SpellId.CryOfDestruction:
                    return Spells.CreateModern(IllustrationName.Deafness, "Cry of Destruction", new Trait[4]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Evocation,
                    Trait.Sonic
                    }, "Your voice booms, smashing what's in front of you.", "Each creature in the area takes " + S.HeightenedVariable(level, 1) + "d8 sonic damage. If you already dealt damage to an enemy this turn, increase the damage dice from this spell to d12s." + S.HeightenedDamageIncrease(level, inCombat, "1d8"), Target.FifteenFootCone(), level, SpellSavingThrow.Basic(Defense.Fortitude)).WithSoundEffect(SfxName.Deafness).WithEffectOnEachTarget(async delegate (CombatAction spell, Creature attacker, Creature defender, CheckResult checkResult)
                    {
                        await CommonSpellEffects.DealBasicDamage(spell, attacker, defender, checkResult, spell.SpellLevel + "d" + (attacker.Actions.DealtDamageToAnotherCreatureThisTurn ? "12" : "8"), DamageKind.Sonic);
                    });
                case SpellId.DazzlingFlash:
                    return Spells.CreateModern(IllustrationName.DazzlingFlash, "Dazzling Flash", new Trait[5]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Evocation,
                    Trait.Light,
                    Trait.Visual
                    }, "You raise your religious symbol and create a blinding flash of light.", "Each creature in the area makes a Fortitude save." + S.FourDegreesOfSuccess("The creature is unaffected.", "The creature is dazzled for 1 round.", "The creature is blinded for 1 round and dazzled for the rest of the encounter. The creature can spend an Interact action rubbing its eyes to end the blinded condition.", "The creature is blinded for 1 round and dazzled for the rest of the encounter."), Target.FifteenFootCone(), level, SpellSavingThrow.Standard(Defense.Fortitude)).WithSoundEffect(SfxName.DazzlingFlash).WithEffectOnEachTarget(async delegate (CombatAction spell, Creature attacker, Creature defender, CheckResult checkResult)
                    {
                        switch (checkResult)
                        {
                            case CheckResult.Success:
                                defender.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(attacker, 1));
                                break;
                            case CheckResult.Failure:
                                {
                                    defender.AddQEffect(QEffect.Dazzled().WithExpirationNever());
                                    QEffect quenchableBlindness = QEffect.Blinded().WithExpirationAtStartOfSourcesTurn(attacker, 1);
                                    quenchableBlindness.ProvideContextualAction = (QEffect qfBlindness) => new ActionPossibility(new CombatAction(qfBlindness.Owner, IllustrationName.RubEyes, "Rub eyes", new Trait[1] { Trait.Manipulate }, "End the blinded condition affecting you because of {i}dazzling flash{/i}.", Target.Self((Creature cr, AI ai) => ai.AlwaysIfSmartAndTakingCareOfSelf)).WithActionCost(1).WithEffectOnSelf(delegate
                                    {
                                        quenchableBlindness.ExpiresAt = ExpirationCondition.Immediately;
                                    })).WithPossibilityGroup("Remove debuff");
                                    defender.AddQEffect(quenchableBlindness);
                                    break;
                                }
                            case CheckResult.CriticalFailure:
                                defender.AddQEffect(QEffect.Dazzled().WithExpirationNever());
                                defender.AddQEffect(QEffect.Blinded().WithExpirationAtStartOfSourcesTurn(attacker, 1));
                                break;
                        }
                    });
                case SpellId.DeathsCall:
                    return Spells.CreateModern(IllustrationName.DeathsCall, "Death's Call", new Trait[3]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Necromancy
                    }, "Seeing another pass from this world to the next invigorates you.", "When a living creature within 20 feet of you dies, or an undead creature within 20 feet of you is destroyed, you can spend your reaction.\n\nIf you do, you gain temporary Hit Points equal to the triggering creature's level plus " + S.SpellcastingModifier(owner, spellInformation) + ". If the triggering creature was undead, double the number of temporary Hit Points you gain.", Target.Uncastable(), level, null).WithActionCost(-2).WithSoundEffect(SfxName.DeathsCall)
                        .WithCastsAsAReaction(delegate (QEffect qfDeathsCall, CombatAction spellItself)
                        {
                            QEffect qfDeathsCall3 = qfDeathsCall;
                            CombatAction spellItself2 = spellItself;
                            QEffect qfDeathsCallTarget3 = new QEffect(ExpirationCondition.Ephemeral)
                            {
                                WhenCreatureDiesAtStateCheckAsync = async delegate (QEffect qfDeathsCallTarget2)
                                {
                                    Creature caster2 = qfDeathsCall3.Owner;
                                    Creature owner2 = qfDeathsCallTarget2.Owner;
                                    int thp = owner2.Level + spellItself2.SpellcastingSource!.SpellcastingAbilityModifier;
                                    if (owner2.HasTrait(Trait.Undead))
                                    {
                                        thp *= 2;
                                    }

                                    if (!owner2.OwningFaction.IsHumanControlled && owner2.DistanceTo(caster2) <= 4 && caster2.Spellcasting!.FocusPoints > 0 && await qfDeathsCall3.Owner.Battle.AskToUseReaction(caster2, "A creature in range died. Spend a focus point and use {i}death's call{/i} to gain " + thp + " temporary HP?"))
                                    {
                                        Sfxs.Play(SfxName.DeathsCall);
                                        caster2.Spellcasting!.UseUpSpellcastingResources(spellItself2);
                                        caster2.GainTemporaryHP(thp);
                                    }
                                }
                            };
                            qfDeathsCall3.StateCheck = delegate (QEffect qfDeathsCall2)
                            {
                                foreach (Creature allCreature in qfDeathsCall2.Owner.Battle.AllCreatures)
                                {
                                    allCreature.AddQEffect(qfDeathsCallTarget3);
                                }
                            };
                        });
                case SpellId.HealersBlessing:
                    return Spells.CreateModern(IllustrationName.HealersBlessing, "Healer's Blessing", new Trait[3]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Necromancy
                    }, "Your words bless a creature with an enhanced connection to positive energy.", "For the rest of the encounter, when the target regains Hit Points from a healing spell, it regains " + S.HeightenedVariable(2 * level, 2) + " additional Hit Points." + S.HeightenText(level, 1, inCombat, "{b}Heightened (+1){/b} The additional healing increases by 2 HP."), Target.RangedFriend(6), level, null).WithActionCost(1).WithSoundEffect(SfxName.Bless)
                        .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                        {
                            int extraHp = spell.SpellLevel * 2;
                            string rulesText = "";
                            rulesText += "When you regain HP from a healing spell, you regain ";
                            rulesText += extraHp.ToString();
                            rulesText += " additional HP.";
                            target.AddQEffect(new QEffect("Healer's Blessing", rulesText, ExpirationCondition.ExpiresAtStartOfSourcesTurn, caster, (Illustration)IllustrationName.HealersBlessing)
                            {
                                CountsAsABuff = true,
                                BonusToSelfHealing = (QEffect qfHealerBlessing, CombatAction? healingSpell) => (healingSpell == null || !healingSpell!.HasTrait(Trait.Spell)) ? null : new Bonus(extraHp, BonusType.Untyped, "Healer's Blessing")
                            }.WithExpirationNever());
                        });
                case SpellId.Moonbeam:
                    SpellSavingThrow savingThrow = null;

                    return Spells.CreateModern(IllustrationName.Moonbeam, "Moonbeam", new Trait[6]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Attack,
                    Trait.Evocation,
                    Trait.Fire,
                    Trait.Light
                    },
                    "You shine a ray of moonlight.", "Make a spell attack roll. The beam of light deals " + S.HeightenedVariable(level, 1) + "d6 fire damage." + S.FourDegreesOfSuccess("Double damage, and the target is dazzled for the rest of the encounter.", "Full damage, and the target is dazzled for 1 round.", null, null) + S.HeightenedDamageIncrease(level, inCombat, "1d6"),
                    Target.Ranged(24),
                    level,
                    null).WithSpellAttackRoll().WithSoundEffect(SfxName.MagicMissile)
                        .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                        {
                            await CommonSpellEffects.DealAttackRollDamage(spell, caster, target, result, spell.SpellLevel + "d6", DamageKind.Fire);
                            if (result >= CheckResult.Success)
                            {
                                QEffect qEffect2 = QEffect.Dazzled();
                                if (result == CheckResult.CriticalSuccess)
                                {
                                    qEffect2.WithExpirationNever();
                                }
                                else
                                {
                                    qEffect2.WithExpirationAtStartOfSourcesTurn(caster, 1);
                                }

                                target.AddQEffect(qEffect2);
                            }
                        });
                case SpellId.TouchOfUndeath:
                    return Spells.CreateModern(IllustrationName.ChillTouch, "Touch of Undeath", new Trait[4]
                    {
                    DawnsburyChampion.ChampionTrait,
                    Trait.Focus,
                    Trait.Necromancy,
                    Trait.Negative
                    }, "You attack the target's life force with undeath.", "You deal " + S.HeightenedVariable(level, 1) + "d6 negative damage. The target attempts a Fortitude save." + S.FourDegreesOfSuccess("The target is unaffected.", "Half damage.", "Full damage, and positive effects heal it only half as much as normal for 1 round.", "Double damage, and positive effects heal it only half as much as normal for the rest of the encounter.") + S.HeightenedDamageIncrease(level, inCombat, "1d6"), Target.Melee().WithAdditionalConditionOnTargetCreature(new LivingCreatureTargetingRequirement()), level, SpellSavingThrow.Basic(Defense.Fortitude)).WithActionCost(1).WithSoundEffect(SfxName.ChillTouch)
                        .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                        {
                            await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, spell.SpellLevel + "d6", DamageKind.Negative);
                            if (result <= CheckResult.Failure)
                            {
                                QEffect qEffect3 = new QEffect("Touched by undeath", "Positive effects heal you only half as much as normal", ExpirationCondition.ExpiresAtStartOfSourcesTurn, caster, (Illustration)IllustrationName.ChillTouch)
                                {
                                    HalveHealingFromEffects = (QEffect qfSelf, CombatAction? healingEffect) => healingEffect?.HasTrait(Trait.Positive) ?? false
                                }.WithExpirationOneRoundOrRestOfTheEncounter(caster, result == CheckResult.CriticalFailure);
                                target.AddQEffect(qEffect3);
                            }
                        });
                default:
                    return null;
            }
        }
    }
}