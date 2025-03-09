using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface ICompanionManagementViewDelegate {
    void ComapnionManagementOnPointerUp(CompanionManagementView companionManagementView, PointerUpEvent evt, Vector2 pointerScreenPosition);
    void CompanionManagementOnClick(CompanionManagementView companionView, ClickEvent evt);
    void CompanionManagementOnPointerDown(CompanionManagementView companionView, PointerDownEvent evt, Vector2 mousePosition);
    void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, PointerMoveEvent evt, Vector2 mousePosition);
    void CompanionManagementOnPointerLeave(CompanionManagementView companionManagementView, PointerLeaveEvent evt);
    void ShowCompanionDeckView(Companion companion);
    void SellCompanion(CompanionManagementView companionView);
    void AddToRoot(VisualElement element);
    bool IsSellingCompanions();
    bool IsDraggingCompanion();
    void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, bool forCompanionManagementView);
    void DestroyTooltip(VisualElement element);
    MonoBehaviour GetMonoBehaviour();
}