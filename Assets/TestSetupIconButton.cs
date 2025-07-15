using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestSetupIconButton : MonoBehaviour
{
    public UIDocument doc;
    public Sprite icon;

    // Start is called before the first frame update
    void Start()
    {
        doc.rootVisualElement.Q<IconButton>().SetIcon(icon);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
