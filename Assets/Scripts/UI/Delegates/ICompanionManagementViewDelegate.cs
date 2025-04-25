using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface ICompanionManagementViewDelegate {
    void ComapnionManagementOnPointerUp(CompanionManagementView companionManagementView, Vector2 pointerPos);
    void CompanionManagementOnClick(CompanionManagementView companionView);
    void CompanionManagementOnPointerDown(CompanionManagementView companionView, Vector2 pointerPos);
    void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, Vector2 mousePosition);
    void CompanionManagementOnPointerLeave(CompanionManagementView companionManagementView, PointerLeaveEvent evt);
    void ShowCompanionDeckView(Companion companion);
    void SellCompanion(CompanionManagementView companionView);
    void AddToRoot(VisualElement element);
    bool IsSellingCompanions();
    bool CanDragCompanions();
    bool IsDraggingCompanion();
    void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, bool forCompanionManagementView);
    void DestroyTooltip(VisualElement element);
    MonoBehaviour GetMonoBehaviour();
}