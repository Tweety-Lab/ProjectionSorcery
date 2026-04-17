using AliLib.Core;
using AliLib.Core.Abilities;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace ProjectionSorcery.Abilities;

public class ProjectionPlottingTintAbility : Ability
{
    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorR = 0.7f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorG = 0.7f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorB = 0.7f;

    /// <summary> The color to tint the screen when in Hack Mode. </summary>
    public static Color PlottingModeColor => new Color(PlottingColorR, PlottingColorG, PlottingColorB);

    /// <summary> The Post Process Volume effects are applied to. </summary>
    public Volume PostProcessVolume { get; set; }

    /// <inheritdoc/>
    public ProjectionPlottingTintAbility(AbilitySpell spell) : base(spell) { }

    private Coroutine? plotTintCoroutine;
    private Coroutine? plotTintUndoCoroutine;

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnEnterPlotting += OnEnterPlotting;
        Spell.GetAbility<ProjectionPlottingAbility>()?.OnExitPlotting += OnExitPlotting;
    }

    /// <inheritdoc/>
    public override void OnEquip()
    {
        base.OnEquip();

        PostProcessVolume = GameObject.FindAnyObjectByType<Volume>();
    }

    public void OnEnterPlotting() => TintScreen();
    public void OnExitPlotting() => UnTintScreen();

    /// <summary> Tints the screen to <see cref="PlottingModeColor"/>. </summary>
    public void TintScreen()
    {
        if (!PostProcessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            colorAdjustments = PostProcessVolume.profile.Add<ColorAdjustments>(true);

        colorAdjustments.colorFilter.overrideState = true;

        if (plotTintUndoCoroutine != null)
            CoroutineRunner.Instance.StopCoroutine(plotTintUndoCoroutine);

        plotTintCoroutine = CoroutineRunner.Instance.PlaySmooth(t => colorAdjustments.colorFilter.Override(Color.Lerp(Color.white, PlottingModeColor, t)), duration: 0.5f);
    }

    /// <summary> Untints the screen. </summary>
    public void UnTintScreen()
    {
        if (PostProcessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            if (plotTintCoroutine != null)
                CoroutineRunner.Instance.StopCoroutine(plotTintCoroutine);

            Color from = colorAdjustments.colorFilter.value;
            plotTintUndoCoroutine = CoroutineRunner.Instance.PlaySmooth(t => colorAdjustments.colorFilter.Override(Color.Lerp(from, Color.white, t)), duration: 0.5f);
        }
    }
}
