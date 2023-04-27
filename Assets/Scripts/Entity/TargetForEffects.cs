using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetForEffects : MonoBehaviour
{
    private PlayableCard playableCard;
    private CompanionInstance companionInstance;
    private EnemyInstance enemyInstance;
    private MinionInstance minionInstance;
    private UICard uiCard;

    // Start is called before the first frame update
    void Start()
    {
        playableCard = GetComponent<PlayableCard>();
        companionInstance = GetComponent<CompanionInstance>();
        enemyInstance = GetComponent<EnemyInstance>();
        minionInstance = GetComponent<MinionInstance>();
        uiCard = GetComponent<UICard>();
    }

    public PlayableCard GetPlayableCard() {
        return playableCard;
    }

    public CompanionInstance GetCompanionInstance() {
        return companionInstance;
    }

    public EnemyInstance GetEnemyInstance() {
        return enemyInstance;
    }

    public MinionInstance GetMinionInstance() {
        return minionInstance;
    }

    public UICard GetUICard() {
        return uiCard;
    }

    public Entity GetEntity() {
        // Only one of the fields in this class should ever be non-null
        if (playableCard != null) {
            return playableCard;
        } else if (companionInstance != null) {
            return companionInstance;
        } else if (enemyInstance != null) {
            return enemyInstance;
        } else if (minionInstance != null) {
            return minionInstance;
        } else if (uiCard != null) {
            return uiCard;
        }
        return null;
    }
}
