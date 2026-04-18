using AliLib.Core;
using AliLib.Core.Abilities;
using ProjectionSorcery.Abilities.Plotting;
using ProjectionSorcery.Abilities.FreezeFrame;
using ProjectionSorcery.Abilities;
using System.Collections.Generic;

namespace ProjectionSorcery;

public class ProjectionSorcerySpell : AbilitySpell
{
    #region JSON
    [ExportedString("Spells/Spell_ProjectionSorcery.json")]
    public const string PROJECTION_SPELL =
        """
        {
          "$type": "ProjectionSorcery.ProjectionSorcerySpell, ProjectionSorcery",
          "id": "ProjectionSorcery",
          "sensitiveContent": "None",
          "sensitiveFilterBehaviour": "Discard",
          "version": 0,
          "groupPath": "",
          "gripCastHeatTransfer": 0.0,
          "allowCastHeat": false,
          "spellCastHeatPerSecond": 0.0,
          "castHeatRadius": 0.0,
          "intensityPerSkill": 0.0,
          "durationPerSkill": 0.0,
          "destroyInWater": false,
          "chargeEffectId": "",
          "readyEffectId": "",
          "readyMinorEffectId": "",
          "allowGripCast": true,
          "fingerEffectId": "",
          "doReadyHaptic": false,
          "chargeSpeed": 1.0,
          "chargeSpeedPerSkill": 0.0,
          "allowStaffBuff": false,
          "grabbedFireMaxCharge": 0.0,
          "endOnGrip": false,
          "allowCharge": true,
          "chargeMinHaptic": 0.0,
          "chargeMaxHaptic": 0.0,
          "handSpringMultiplier": 1.0,
          "handLocomotionVelocityCorrectionMultiplier": 1.0,
          "allowThrow": false,
          "throwEffectId": "",
          "throwMinCharge": 0.0,
          "allowSpray": false,
          "gripCastEffectId": "",
          "imbueEnabled": false,
          "wheelDisplayName": "Projection Sorcery",
          "hasOrder": true,
          "order": 0,
          "iconEffectId": "SpellOrbLightning",
          "loopMaxDuration": 5.0,
          "minMana": 0.0,
          "meshAddress": "Bas.Mesh.SkillTree.TierCrystal.Fire.T1",
          "meshSize": 0.6,
          "tier": 0,
          "allowSkill": true,
          "forceAllowRefund": false,
          "showInTree": true,
          "hideInSkillMenu": false,
          "skillTreeDisplayName": "Projection Sorcery",
          "description": "Projection Sorcery.",
          "imageAddress": "",
          "videoAddress": null,
          "buttonSpriteSheetAddress": "Bas.Ui.SkillTree.Icons",
          "buttonEnabledIconAddress": "",
          "buttonDisabledIconAddress": "",
          "orbIconAddress": "",
          "costOverride": -1,
          "isDefaultSkill": true,
          "primarySkillTreeId": "Mind",
          "secondarySkillTreeId": "",
          "isTierBlocker": false,
          "canSpawnAsReward": true,
          "allowInRouletteMode": true,
          "combatSkill": true,
          "allowOnFirstWave": true,
          "requiredSkills": [],
          "Order": 0,
          "IsCombinedSkill": false
        }
        """;
    #endregion

    /// <inheritdoc />
    public override List<Ability> RegisterAbilities()
    {
        return new List<Ability>
        {
            new ProjectionAbility(this),
            new ProjectionTintAbility(this),
            new ProjectionOutlineAbility(this),
            new ProjectionTimeFreezeAbility(this),
            new ProjectionInteractionAbility(this),
            new ProjectionSoudAbility(this),
            new ProjectionFreezeFrameAbility(this)
        };
    }
}
