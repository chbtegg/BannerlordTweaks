using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerlordTweaks
{
    /// <summary>
    /// Allows for tweaking of the pregnancy Model
    /// NOTE: we set the probabilities for disabling to -1 since TW does a smaller-equals check on a range of [0,1)
    /// </summary>
    public class TweakedPregnancyModel : DefaultPregnancyModel
    {
        public override float StillbirthProbability => Settings.Instance.NoStillbirthsTweakEnabled 
            ? 0.0f 
            : base.StillbirthProbability;

        public override float MaternalMortalityProbabilityInLabor => Settings.Instance.NoMaternalMortalityTweakEnabled
            ? 0.0f
            : base.MaternalMortalityProbabilityInLabor;

        public override float DeliveringTwinsProbability => Settings.Instance.TwinsProbabilityTweakEnabled
            ? Settings.Instance.TwinsProbability
            : base.DeliveringTwinsProbability;

        public override float DeliveringFemaleOffspringProbability => Settings.Instance.FemaleOffspringProbabilityTweakEnabled
            ? Settings.Instance.FemaleOffspringProbability
            : base.DeliveringFemaleOffspringProbability;

        public override float PregnancyDurationInDays => Settings.Instance.PregnancyDurationTweakEnabled
            ? Settings.Instance.PregnancyDuration
            : base.PregnancyDurationInDays;

        public override float CharacterFertilityProbability => Settings.Instance.CharacterFertilityProbabilityTweakEnabled
            ? Settings.Instance.CharacterFertilityProbability
            : base.CharacterFertilityProbability;

        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            if (!Settings.Instance.DailyChancePregnancyTweakEnabled)
                return base.GetDailyChanceOfPregnancyForHero(hero);

            float num = 0.0f;
            if (!Settings.Instance.PlayerCharacterFertileEnabled && CheckIfHeroIsMainOrSpouseIsMarriedToPlayerHero(hero))
            {
                return num;
            }

            if (Settings.Instance.MaxChildrenTweakEnabled && hero.Children != null && hero.Children.Any() && hero.Children.Count > Settings.Instance.MaxChildren)
            {
                return num;
            }

            if (hero.Spouse != null && hero.IsFertile && IsHeroAgeSuitableForPregnancy(hero))
            {
                ExplainedNumber bonuses = new ExplainedNumber(1f, (StringBuilder)null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.PerfectHealth, hero.Clan.Leader.CharacterObject, ref bonuses);
                num = (float)((6.5 - ((double)hero.Age - Settings.Instance.MinPregnancyAge) * 0.230000004172325) * 0.0199999995529652) * bonuses.ResultNumber;
            }

            if (hero.Children == null || !hero.Children.Any())
                num *= 3f;
            else if (hero.Children.Count > 1)
                num *= 2f;

            return num;
        }

        private bool IsHeroAgeSuitableForPregnancy(Hero hero)
        {
            if ((double)hero.Age >= Settings.Instance.MinPregnancyAge)
                return (double)hero.Age <= Settings.Instance.MaxPregnancyAge;
            return false;
        }

        private bool CheckIfHeroIsMainOrSpouseIsMarriedToPlayerHero(Hero hero)
        {
            if (hero.CharacterObject != null && hero.CharacterObject.IsPlayerCharacter)
                return true;

            if (hero.Spouse?.CharacterObject != null && hero.Spouse.CharacterObject.IsPlayerCharacter)
                return true;

            if (hero.Spouse?.Spouse?.CharacterObject != null && hero.Spouse.Spouse.CharacterObject.IsPlayerCharacter)
                return true;

            return false;
        }
    }
}
