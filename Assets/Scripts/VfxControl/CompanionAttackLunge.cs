using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
    The companion lunges at the enemy it's attacking, the way enemies lunge at companions
    (EnemyAttackVFX / GenericEnemyAttackEffectStep), but with no per-card setup.

    CombatEffectStep calls Play when an attack card a companion owns deals damage to enemies. The companion's
    sprite moves part of the way toward its target(s) (the middle of them for multi-target attacks), and Play
    returns the seconds until it arrives so the step can land the hit then. A companion already mid-lunge (a
    second damage step on the same card, Double Strike etc.) doesn't lunge again.

    Tuning and the off switch: Encounter Constants > Companion Attack Lunge.
*/
public static class CompanionAttackLunge
{
    private static readonly HashSet<VisualElement> lunging = new HashSet<VisualElement>();

    public static float Play(CombatInstance attacker, List<CombatInstance> targets)
    {
        EncounterConstantsSO constants = CombatEntityManager.Instance.encounterConstants;
        if (constants == null || !constants.useCompanionAttackLunge) return 0f;
        if (attacker == null || attacker.GetComponentInParent<CompanionInstance>() == null) return 0f;

        VisualElement sprite = attacker.GetVisualElement();
        if (sprite == null || sprite.panel == null || lunging.Contains(sprite)) return 0f;

        Vector2 aim = Vector2.zero;
        int enemyCount = 0;
        foreach (CombatInstance target in targets)
        {
            if (target == null || target.GetComponentInParent<EnemyInstance>() == null) continue;
            VisualElement targetElement = target.GetVisualElement();
            if (targetElement == null || targetElement.panel == null) continue;
            aim += targetElement.worldBound.center;
            enemyCount++;
        }
        if (enemyCount == 0) return 0f;
        aim /= enemyCount;

        Vector3 home = sprite.transform.position;
        Vector3 offset = (Vector3)((aim - sprite.worldBound.center) * constants.companionLungeDistance);
        float outTime = Mathf.Max(0.01f, constants.companionLungeOutTime);
        float returnTime = Mathf.Max(0.01f, constants.companionLungeReturnTime);

        lunging.Add(sprite);
        LeanTween.value(0f, 1f, outTime)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float t) => sprite.transform.position = home + offset * t)
            .setOnComplete(() =>
                LeanTween.value(1f, 0f, returnTime)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float t) => sprite.transform.position = home + offset * t)
                    .setOnComplete(() =>
                    {
                        sprite.transform.position = home;
                        lunging.Remove(sprite);
                    }));

        return outTime;
    }
}
