using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

/* could come back to this if the method of selecting the step you want to add actions
to becomes too much of a pain in the booty. Right now I'm getting this from the console,
so we're leaving behind some class information somewhere:

error CS0030: Cannot convert type 'UnityEngine.Object' to 'UnityEventTutorialStep'
*/
/*
[CustomEditor(typeof(TutorialStep))]
public class TutorialStepEditor : Editor {

    public override void OnInspectorGUI() {
        TutorialStep step = (UnityEventTutorialStep) target;
        DrawDefaultInspector();

    }

}
*/
