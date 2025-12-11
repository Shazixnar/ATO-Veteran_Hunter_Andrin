using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using Obeliskial_Essentials;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;

namespace TraitMod
{
    [HarmonyPatch]
    internal class Traits
    {

        public static string[] myTraitList = { "shazixnarmasterofmeleeandranged", "shazixnarmasteroftrack", "shazixnartracker" };

        private static readonly Traits _instance = new Traits();

        public static void myDoTrait(
            string trait,
            Enums.EventActivation evt,
            Character character,
            Character target,
            int auxInt,
            string auxString,
            CardData castedCard)
        {
            if (character == null) return;

            switch (trait)
            {
                case "shazixnarmasterofmeleeandranged":
                    _instance.shazixnarmasterofmeleeandranged(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;

                case "shazixnarmasteroftrack":
                    _instance.shazixnarmasteroftrack(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;

                case "shazixnartracker":
                    _instance.shazixnartracker(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;
            }
        }

            // activate traits
        public void shazixnarmasterofmeleeandranged(
            Enums.EventActivation evt,
            Character character,
            Character target,
            int auxInt,
            string auxString,
            CardData castedCard,
            string trait)
        {
            if (character == null || castedCard == null) return;

            TraitData data = Globals.Instance.GetTraitData(trait);
            int used = MatchManager.Instance.activatedTraits.ContainsKey(trait) ? MatchManager.Instance.activatedTraits[trait] : 0;
            if (used >= data.TimesPerTurn) return;

            bool isMelee = castedCard.GetCardTypes().Contains(Enums.CardType.Melee_Attack);
            bool isRanged = castedCard.GetCardTypes().Contains(Enums.CardType.Ranged_Attack);

            if (!isMelee && !isRanged) return;

            MatchManager.Instance.activatedTraits[trait] = used + 1;
            MatchManager.Instance.SetTraitInfoText();

            // 显示 combat text
            character.HeroItem?.ScrollCombatText(
                Texts.Instance.GetText("traits_Master of Melle and Ranged", "")
                + Functions.TextChargesLeft(used + 1, data.TimesPerTurn),
                Enums.CombatScrollEffectType.Trait
            );

            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (isRanged)
            {
                // 返还能量
                character.ModifyEnergy(1, true);

                EffectsManager.Instance.PlayEffectAC("energy", true, character.HeroItem?.CharImageT, false, 0f);

                // NPC 上标记
                foreach (var npc in teamNpc)
                {
                    if (npc != null && npc.Alive)
                        npc.SetAuraTrait(character, "mark", 2);
                }

                // 获得一张近战卡
                string newCardId = "";
                CardData newCard = null;
                do
                {
                    int randPercent = MatchManager.Instance.GetRandomIntRange(0, 100, "trait", "");
                    int randIndex = MatchManager.Instance.GetRandomIntRange(0, Globals.Instance.CardListByType[Enums.CardType.Melee_Attack].Count, "trait", "");
                    string id = Globals.Instance.CardListByType[Enums.CardType.Melee_Attack][randIndex];
                    id = Functions.GetCardByRarity(randPercent, Globals.Instance.GetCardData(id, false), false);
                    newCardId = MatchManager.Instance.CreateCardInDictionary(id, "", false);
                    newCard = MatchManager.Instance.GetCardData(newCardId);
                } while (newCard.CardClass.ToString() == "Monster");

                newCard.Vanish = true;
                newCard.EnergyReductionPermanent = 3;
                MatchManager.Instance.GenerateNewCard(1, newCardId, false, Enums.CardPlace.Hand, null, null, -1, true, 0);
                MatchManager.Instance.ItemTraitActivated(true);
                MatchManager.Instance.CreateLogCardModification(newCard.InternalId, MatchManager.Instance.GetHero(character.HeroIndex));
            }
            else if (isMelee)
            {
                // 抽一牌
                MatchManager.Instance.NewCard(1, Enums.CardFrom.Deck, "");

                // 获得 1 强效
                character.SetAuraTrait(character, "powerful", 1);

                // 获得一张远程卡
                string newCardId = "";
                CardData newCard = null;
                do
                {
                    int randPercent = MatchManager.Instance.GetRandomIntRange(0, 100, "trait", "");
                    int randIndex = MatchManager.Instance.GetRandomIntRange(0, Globals.Instance.CardListByType[Enums.CardType.Ranged_Attack].Count, "trait", "");
                    string id = Globals.Instance.CardListByType[Enums.CardType.Ranged_Attack][randIndex];
                    id = Functions.GetCardByRarity(randPercent, Globals.Instance.GetCardData(id, false), false);
                    newCardId = MatchManager.Instance.CreateCardInDictionary(id, "", false);
                    newCard = MatchManager.Instance.GetCardData(newCardId);
                } while (newCard.CardClass.ToString() == "Monster");

                newCard.Vanish = true;
                newCard.EnergyReductionPermanent = 3;
                MatchManager.Instance.GenerateNewCard(1, newCardId, false, Enums.CardPlace.Hand, null, null, -1, true, 0);
                MatchManager.Instance.ItemTraitActivated(true);
                MatchManager.Instance.CreateLogCardModification(newCard.InternalId, MatchManager.Instance.GetHero(character.HeroIndex));
            }
        }

        public void shazixnarmasteroftrack(
            Enums.EventActivation evt,
            Character character,
            Character target,
            int auxInt,
            string auxString,
            CardData castedCard,
            string trait)
        {
            if (character == null || castedCard == null) return;

            TraitData data = Globals.Instance.GetTraitData(trait);
            int used = MatchManager.Instance.activatedTraits.ContainsKey(trait) ? MatchManager.Instance.activatedTraits[trait] : 0;
            if (used >= data.TimesPerTurn) return;

            if (!castedCard.HasCardType(Enums.CardType.Skill) || character.HeroData == null)
                return;

            MatchManager.Instance.activatedTraits[trait] = used + 1;
            MatchManager.Instance.SetTraitInfoText();

            // 显示 combat text
            character.HeroItem?.ScrollCombatText(
                Texts.Instance.GetText("traits_Master of Track", "")
                + Functions.TextChargesLeft(used + 1, data.TimesPerTurn),
                Enums.CombatScrollEffectType.Trait
            );

            // 抽一张新卡（从 Deck）
            MatchManager.Instance.NewCard(1, Enums.CardFrom.Deck, "");

            // 队伍 NPC 增加 sight 光环
            NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();
            foreach (var npc in teamNPC)
            {
                if (npc != null && npc.Alive)
                    npc.SetAuraTrait(character, "sight", 2);
            }
        }

        public void shazixnartracker(
            Enums.EventActivation evt,
            Character character,
            Character target,
            int auxInt,
            string auxString,
            CardData castedCard,
            string trait)
        {
            if (character == null || character.HeroData == null) return;

            NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();
            foreach (var npc in teamNPC)
            {
                if (npc != null && npc.Alive)
                {
                    npc.SetAuraTrait(character, "mark", 1);

                    if (npc.NPCItem != null)
                    {
                        npc.NPCItem.ScrollCombatText(
                            Texts.Instance.GetText("traits_Tracker", ""),
                            Enums.CombatScrollEffectType.Trait
                        );
                    }
                }
            }

            // 可选：如果需要显示 trait 次数，也可以像其他 trait 一样加上计数
        }

        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static class Trait_DoTrait_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(
                Enums.EventActivation __0,   // theEvent
                string __1,                  // trait id
                Character __2,               // character
                Character __3,               // target
                int __4,                     // auxInt
                string __5,                  // auxString
                CardData __6,                // castedCard
                Trait __instance)
            {
                string trait = __1;

                // 如果是自定义 trait，就直接调用我们的逻辑
                if (myTraitList.Contains(trait))
                {
                    myDoTrait(
                        trait,
                        __0,        // event
                        __2,        // character
                        __3,        // target
                        __4,        // auxInt
                        __5,        // auxString
                        __6         // castedCard
                    );

                    // 返回 false = 阻止原版 DoTrait 执行
                    return false;
                }

                // 否则走原版逻辑
                return true;
            }
        }

        public static string TextChargesLeft(int currentCharges, int chargesTotal)
        {
            int cCharges = currentCharges;
            int cTotal = chargesTotal;
            return "<br><color=#FFF>" + cCharges.ToString() + "/" + cTotal.ToString() + "</color>";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            bool flag = false;
            bool flag2 = false;
            if (_characterCaster != null && _characterCaster.IsHero)
            {
                flag = _characterCaster.IsHero;
            }
            if (_characterTarget != null && _characterTarget.IsHero)
            {
                flag2 = true;
            }
            if (_acId == "sight")
            {
                if (_type == "set")
                {
                    if (!flag2)
                    {
                        if (__instance.TeamHaveTrait("shazixnarmasteroftrack"))
                        {
                            __result.AuraDamageIncreasedPercentPerStack = -0.2f;
                            __result.ResistModifiedPercentagePerStack = -0.2f;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.ResistModified = Enums.DamageType.All;
                        }
                    }
                }
            }
            if (_acId == "mark")
            {
                if (_type == "set")
                {
                    if (!flag2)
                    {
                        if (__instance.TeamHaveTrait("shazixnarmarkofdeath"))
                        {
                            __result.ResistModifiedPercentagePerStack = -0.2f;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.MaxMadnessCharges = 75;
                        }
                    }
                }
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetEvent")]
        public static void SetEventPrefix(ref Character __instance, ref Enums.EventActivation theEvent, Character target = null)
        {
            if (theEvent == Enums.EventActivation.Hitted
                    && !__instance.IsHero
                    && target.HaveTrait("shazixnarmercilesshunter")
                    && __instance.HasEffect("mark"))
            {
                // 有无情猎手天赋时，敌人带标记，被安德兰击中时承受标记层数伤害，流血增加倍率。
                float num = 2;
                num *= (1 + __instance.GetAuraCharges("bleed") * 0.01f);
                Enums.DamageType[] types = { Enums.DamageType.Slashing, Enums.DamageType.Piercing };
                Enums.DamageType damageType = types[UnityEngine.Random.Range(0, types.Length)];
                __instance.IndirectDamage(damageType, Functions.FuncRoundToInt((float)__instance.GetAuraCharges("mark") * num));
            }
        }
    }
}
