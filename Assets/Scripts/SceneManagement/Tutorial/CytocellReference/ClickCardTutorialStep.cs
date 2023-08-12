// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [RequireComponent(typeof(TextGroupAlphaControls))]
// [RequireComponent(typeof(EffectTargetRequestEventListener))]
// public class ClickCardTutorialStep: MonoBehaviour
// {
//     private TextGroupAlphaControls alphaControls;
//     private bool hasClicked = false;
    

//     void Start() {
//         alphaControls = GetComponent<TextGroupAlphaControls>();
//     }

//     public void ClickCardStep(){
//         StartCoroutine("corout");
//     }

//     public void OnEffectTargetRequest(EffectTargetRequestEventInfo request){
//         hasClicked = true;
//     }



//     private IEnumerator corout(){
//         print("calling click card display all");
//         alphaControls.displayAll();
//         yield return new WaitUntil(() => hasClicked);
//         yield return new WaitForSeconds(1f);
//         alphaControls.hideAll();
//         SendMessageUpwards("StepDone");
//     }
// }