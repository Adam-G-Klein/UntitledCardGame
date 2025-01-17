using System;
using UnityEngine.UIElements;

public class CompanionView {
    public VisualElement companionContainer;

    public CompanionView(Companion companion) {
        companionContainer = makeCompanion(companion);
    }

    private VisualElement makeCompanion(Companion companion) {
        EntityView entityView = new EntityView(companion, 0, false);

        VisualElement portraitContainer = entityView.entityContainer.Q(className: "portrait-container");
        portraitContainer.style.backgroundImage = new StyleBackground(companion.companionType.sprite);

        return entityView.entityContainer;
    }
}