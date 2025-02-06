using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface ICompanionManagementViewDelegate {
    void ComapnionManagementOnPointerUp(CompanionManagementView companionManagementView, PointerUpEvent evt);
    void CompanionManagementOnClick(CompanionManagementView companionView, ClickEvent evt);
    void CompanionManagementOnPointerDown(CompanionManagementView companionView, PointerDownEvent evt);
    void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, PointerMoveEvent evt);
    void CompanionManagementOnPointerLeave(CompanionManagementView companionManagementView, PointerLeaveEvent evt);
    void ShowCompanionDeckView(Companion companion);
    void SellCompanion(Companion companion);
    void AddToRoot(VisualElement element);
    bool IsSellingCompanions();
    bool IsDraggingCompanion();
    MonoBehaviour GetMonoBehaviour();
}