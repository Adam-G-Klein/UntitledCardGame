using UnityEngine;
using TMPro;

[RequireComponent(typeof(TurnPhaseEventListener))]
public class ManaManager : GenericSingleton<ManaManager> {
    public int _currentMana = 3;
    public int currentMana { get {
            return _currentMana;
        }set {
            listener.GetComponent<CombatEncounterView>().updateMana(value);
            _currentMana = value;

        }
    }
    public int extraManaNextTurn = 0;
    public int manaPerTurn;
    public GameObject listener;
    private TextMeshProUGUI text;

    void Start() {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update() {
        if (GameplayConstantsSingleton.Instance.gameplayConstants.DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.M)) {
            currentMana += 1;
        }
    }

    public void manaEventHandler(int info) {
        currentMana += info;
    }

    public void updateMana(int change) {
        currentMana += change;
    }

    public void SetManaPerTurn(int num) {
        this.manaPerTurn = num;
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.START_PLAYER_TURN) {
            currentMana = manaPerTurn + extraManaNextTurn;
            extraManaNextTurn = 0;
        }
    }

    public void AddExtraManaNextTurn(int mana) {
        extraManaNextTurn = mana;
    }
}