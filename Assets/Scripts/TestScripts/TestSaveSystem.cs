using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
At the end of this, we'll want to be saving and loading one "gameSave" object, which will contain all the data we want to save.

*/
public class TestSaveSystem : MonoBehaviour
{
    public UIDocument uidoc;
    private TextField textField;
    private Button saveButton;
    private Button loadButton;
    // Start is called before the first frame update
    void Start()
    {
        textField = uidoc.rootVisualElement.Q<TextField>("SaveSystemTextField");
        if (textField == null) {
            Debug.LogError("[SaveTest] textField is null!");
        } else {
            Debug.Log("[SaveTest] textField found: " + textField.name);
        }
        saveButton = uidoc.rootVisualElement.Q<Button>("Save");
        loadButton = uidoc.rootVisualElement.Q<Button>("Load");

        saveButton.RegisterCallback<ClickEvent>(ev => SaveButtonHandler());
        loadButton.RegisterCallback<ClickEvent>(ev => LoadButtonHandler());
        
    }

    void SaveButtonHandler() {
        SaveSystem.Save<string>(textField.text);
        Debug.Log("[SaveTest] textField contents: " + textField.text);
    }
    void LoadButtonHandler() {
        string loadedText = SaveSystem.Load<string>();
        textField.value = loadedText;
        Debug.Log("[SaveTest] Loaded text: " + loadedText);
    }
}
