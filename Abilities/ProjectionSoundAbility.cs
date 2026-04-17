using AliLib.Core;
using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

public class ProjectionSoudAbility : Ability
{
    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Sounds", 1)]
    [ModOptionFloatValues(0f, 1f, 0.1f)]
    public static float PlottingEnterVolume = 1f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Sounds", 1)]
    [ModOptionFloatValues(0f, 1f, 0.1f)]
    public static float PlottingExitVolume = 1f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Sounds", 1)]
    [ModOptionFloatValues(0f, 1f, 0.1f)]
    public static float PlaybackTickVolume = 1f;

    [Addressable("ProjectionSorcery.Sounds.Woosh")]
    public static AudioClip? PlottingEnterSound { get; set; }

    [Addressable("ProjectionSorcery.Sounds.WooshReverse")]
    public static AudioClip? PlottingExitSound { get; set; }

    [Addressable("ProjectionSorcery.Sounds.Shutter")]
    public static AudioClip? PlaybackTickSound { get; set; }

    /// <inheritdoc/>
    public ProjectionSoudAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionAbility>()?.OnEnterPlotting += () =>
        {
            if (PlottingEnterSound == null)
                return;

            Audio.PlayNoBlend(PlottingEnterSound, ThunderRoad.AudioMixerName.Effect, PlottingEnterVolume);
        };

        Spell.GetAbility<ProjectionAbility>()?.OnExitPlotting += () =>
        {
            if (PlottingExitSound == null)
                return;

            Audio.PlayNoBlend(PlottingExitSound, ThunderRoad.AudioMixerName.Effect, PlottingExitVolume);
        };

        Spell.GetAbility<ProjectionAbility>()?.OnPlaybackTick += (frame) =>
        {
            if (PlaybackTickSound == null)
                return;

            Audio.PlayNoBlend(PlaybackTickSound, ThunderRoad.AudioMixerName.Effect, PlaybackTickVolume);
        };
    }
}
