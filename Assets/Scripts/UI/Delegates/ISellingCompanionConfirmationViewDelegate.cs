using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface ISellingCompanionConfirmationViewDelegate {
    void ConfirmSellCompanion();
    void StopSellingCompanion();
    CompanionSellValue CalculateCompanionSellPrice(Companion companion);
}