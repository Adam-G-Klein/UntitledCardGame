using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Turn phases:
Draw card from each companion
Player plays cards until end turn is pressed
Enemies deal damage
*/
public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("LateStart");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator LateStart() {
        yield return new WaitForEndOfFrame();
        turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.ENEMY_ATTACK));



    }
}