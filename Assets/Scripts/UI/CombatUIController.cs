using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatUIController : MonoBehaviour
{
    [SerializeField] 
    private TurnPhaseEvent turnPhaseEvent;
    private bool endTurnButtonEnabled = true;
    private Label manaCounter;

    public CombatEntityManager combatEntityManager;
    public UIDocument combatUI;
    public List<VisualElement> friendlies = new List<VisualElement>();
    public List<VisualElement> enemies = new List<VisualElement>();
    // Start is called before the first frame update
    void Start()
    {
        combatUI = GetComponent<UIDocument>();
        if (combatUI == null)
        {
            Debug.LogError("no combat UI found");
        }
        //TODO: Factor UI setup into VVM pattern also... dedup this shitty logic.
        var enemies = combatEntityManager.getEnemies();
        var companions = combatEntityManager.getCompanions();
        var companionListView = combatUI.rootVisualElement.Q("character-portraits");
        var enemyListView = combatUI.rootVisualElement.Q("enemy-portraits");
        for (int i = 0; i < companions.Count; i++) {
            var portrait = new VisualElement();
            portrait.name = "companion" + i;
            portrait.AddToClassList("character-portrait");
            var health = new Label();
            health.name = "Health";
            var hp = companions[i].companion.GetCombatStats().getCurrentHealth();
            var maxHp = companions[i].companion.GetCombatStats().maxHealth;
            health.text = hp + "/" + maxHp;
            health.AddToClassList("health-text");
            portrait.Add(health);
            friendlies.Add(portrait);
            companionListView.Add(portrait);
            //BindCompanion(companions[i], portrait);
            
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            var portrait = new VisualElement();
            portrait.name = "enemy" + i;
            portrait.AddToClassList("character-portrait");
            var health = new Label();
            health.name = "Health";
            var hp = enemies[i].enemy.GetCombatStats().getCurrentHealth();
            var maxHp = enemies[i].enemy.GetCombatStats().maxHealth;
            health.text = hp + "/" + maxHp;
            health.AddToClassList("health-text");
            portrait.Add(health);
            this.enemies.Add(portrait);
            enemyListView.Add(portrait);
            //BindEnemy(enemies[i].enemy.GetCombatStats(), i);
        }
        combatUI.rootVisualElement.Q<Button>("end-turn-button").clicked += endTurnButtonHandler;
    }

    // Update is called once per frame
    void Update()
    {
        //TODO make these update functions run on a subscribe, not on update;
        UpdateCompanionUI();
        UpdateEnemyUI();
    }

    private void UpdateEnemyUI() 
    {
        var enemies = combatEntityManager.getEnemies();
        for (int i = 0; i < this.enemies.Count; i++)
        {
            if (enemies.Count <= i)
            {
                (this.enemies[i].Q("Health") as Label).text = "";
                this.enemies[i].style.backgroundImage = null;
            }
            else
            {
                var hp = enemies[i].enemy.GetCombatStats().getCurrentHealth();
                var maxHp = enemies[i].enemy.GetCombatStats().maxHealth;
                
                this.enemies[i].style.backgroundImage = new StyleBackground(enemies[i].enemy.enemyType.sprite);
                (this.enemies[i].Q("Health") as Label).text = hp + "/" + maxHp;
            }
        }
    }

    private void UpdateCompanionUI()
    {
        var companions = combatEntityManager.getCompanions();
        for (int i = 0; i < friendlies.Count; i++)
        {
            if (companions.Count <= i)
            {
                (friendlies[i].Q("Health") as Label).text = "";
                friendlies[i].style.backgroundImage = null;
            }
            else
            {
                var hp = companions[i].companion.GetCombatStats().getCurrentHealth();
                var maxHp = companions[i].companion.GetCombatStats().maxHealth;

                (friendlies[i].Q("Health") as Label).text = hp + "/" + maxHp;
                friendlies[i].style.backgroundImage = new StyleBackground(companions[i].companion.companionType.sprite);
            }
        }
    }


    private void endTurnButtonHandler() {
        if(endTurnButtonEnabled)
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        if(info.newPhase == TurnPhase.PLAYER_TURN)
        {
            endTurnButtonEnabled = true;
        }
        else
        {
            endTurnButtonEnabled = false;
        }

    }

    

/*    public void BindCompanion(CompanionInstance companion, VisualElement portrait)
    {
        // Create the SerializedObject from the current selection
        SerializedObject so = new SerializedObject(companion);

        // Note: the "name" property of a GameObject is actually named "m_Name" in serialization.
        SerializedProperty curhealthProp = so.FindProperty("m_companion.m_combatStats.m_currentHealth");
        SerializedProperty maxHealthProp = so.FindProperty("m_companion.m_combatStats.m_maxHealth");

        // Ensure to use Unbind() before tracking a new property
        portrait.Unbind();
        portrait.TrackPropertyValue(curhealthProp, updateHPLabel);
        portrait.TrackPropertyValue(maxHealthProp, updateHPLabel);
        var label = portrait.style.backgroundImage;
        label.
        // Bind the property to the field directly
        label.BindProperty(so);

        CheckName(property);
    }
    }*/
}
