using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/*
    The "this just triggered" cue for automatic effects (a unit's passive, a relic/power): a small pill with the
    source's icon and name pops up over the unit, drifts up and fades. When the source has an icon that is also
    shown in the unit's status row (relics are), that icon pulses too, so it's clear which one fired.
    Runs on the unit's own panel and on game time, so it keeps pace with the game speed setting.
*/
public static class AbilityTriggerBadge
{
    const float PopIn = 0.14f, Settle = 0.1f, Hold = 0.7f, FadeOut = 0.3f;
    const float Rise = 26f;
    static readonly Color Fill = new Color(0.08f, 0.055f, 0.1f, 0.9f);
    static readonly Color Rim = new Color(0.91f, 0.77f, 0.42f, 1f);
    static readonly Color Text = new Color(1f, 0.96f, 0.84f, 1f);

    public static IEnumerator Show(VisualElement anchor, Sprite icon, string label, VisualElement statusArea = null)
    {
        if (anchor == null || anchor.panel == null || string.IsNullOrEmpty(label)) yield break;
        VisualElement root = VisualElementUtils.GetRootVisualElement(anchor);

        VisualElement badge = new VisualElement { pickingMode = PickingMode.Ignore };
        badge.style.position = Position.Absolute;
        badge.style.flexDirection = FlexDirection.Row;
        badge.style.alignItems = Align.Center;
        badge.style.paddingLeft = 10; badge.style.paddingRight = 12; badge.style.paddingTop = 4; badge.style.paddingBottom = 4;
        badge.style.backgroundColor = Fill;
        badge.style.borderTopLeftRadius = 16; badge.style.borderTopRightRadius = 16; badge.style.borderBottomLeftRadius = 16; badge.style.borderBottomRightRadius = 16;
        badge.style.borderTopWidth = 2; badge.style.borderBottomWidth = 2; badge.style.borderLeftWidth = 2; badge.style.borderRightWidth = 2;
        badge.style.borderTopColor = Rim; badge.style.borderBottomColor = Rim; badge.style.borderLeftColor = Rim; badge.style.borderRightColor = Rim;
        if (icon != null)
        {
            VisualElement iconElement = new VisualElement { pickingMode = PickingMode.Ignore };
            iconElement.style.width = 28; iconElement.style.height = 28;
            iconElement.style.marginRight = 6;
            iconElement.style.backgroundImage = new StyleBackground(icon);
            iconElement.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            badge.Add(iconElement);
        }
        Label text = new Label(label) { pickingMode = PickingMode.Ignore };
        text.style.fontSize = 20;
        text.style.color = Text;
        text.style.unityFontStyleAndWeight = FontStyle.Bold;
        text.style.marginLeft = 0; text.style.marginRight = 0; text.style.paddingLeft = 0; text.style.paddingRight = 0;
        badge.Add(text);

        // centred just above the unit
        Rect at = anchor.worldBound;
        float top = at.yMin - 8f;
        badge.style.left = at.center.x;
        badge.style.top = top;
        badge.style.translate = new Translate(Length.Percent(-50), Length.Percent(-100));
        badge.style.opacity = 0f;
        root.Add(badge);

        if (statusArea != null && icon != null) PulseMatchingIcon(statusArea, icon);

        float t = 0f, total = PopIn + Settle + Hold + FadeOut;
        while (t < total)
        {
            t += Time.deltaTime;
            float scale = t < PopIn ? Mathf.Lerp(0.6f, 1.08f, t / PopIn)
                        : t < PopIn + Settle ? Mathf.Lerp(1.08f, 1f, (t - PopIn) / Settle)
                        : 1f;
            float fade = t > total - FadeOut ? 1f - (t - (total - FadeOut)) / FadeOut : Mathf.Clamp01(t / PopIn);
            badge.style.scale = new Scale(new Vector3(scale, scale, 1f));
            badge.style.opacity = fade;
            badge.style.top = top - Rise * Mathf.SmoothStep(0f, 1f, t / total);
            yield return null;
        }
        badge.RemoveFromHierarchy();
    }

    // the status icon showing this sprite gets a quick double pulse
    static void PulseMatchingIcon(VisualElement statusArea, Sprite icon)
    {
        statusArea.Query<VisualElement>().ForEach(e =>
        {
            if (e.style.backgroundImage.value.sprite != icon) return;
            float t = 0f;
            e.schedule.Execute(timer =>
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.5f);
                float s = p >= 1f ? 1f : 1f + 0.45f * Mathf.Abs(Mathf.Sin(p * Mathf.PI * 2f)) * (1f - p);
                e.style.scale = new Scale(new Vector3(s, s, 1f));
            }).Every(16).Until(() => t >= 0.5f);
        });
    }
}
