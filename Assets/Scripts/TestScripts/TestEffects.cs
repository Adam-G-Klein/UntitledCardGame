using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEffects : MonoBehaviour
{
    [SerializeReference]
    public List<EffectStep> effects = new List<EffectStep>();

    public void testEffects() {
        EffectDocument document = new EffectDocument();
        document.originEntityType = EntityType.PlayableCard;
        document.playableCardMap.addItem(EffectDocument.ORIGIN, PlayerHand.Instance.cardsInHand[0]);

        StartCoroutine(effectWorkflowCoroutine(effects, document));
    }

    private IEnumerator effectWorkflowCoroutine(
            List<EffectStep> effects,
            EffectDocument document) {
        IEnumerator currentCoroutine;
        foreach (EffectStep effect in effects) {
            currentCoroutine = effect.invoke(document);
            yield return StartCoroutine(currentCoroutine);
        }
        yield return null;
    }
}
