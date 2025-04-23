using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface ISellingCompanionConfirmationViewDelegate {
    void ConfirmSellCompanion();
    void StopSellingCompanion();
    int CalculateCompanionSellPrice(Companion companion);
}