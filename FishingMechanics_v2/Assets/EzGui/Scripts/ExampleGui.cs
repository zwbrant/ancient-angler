using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleGui : MonoBehaviour
{
    string guiText = "hello";
    float fittedGuiFloat;
    float guiFloat;
    int fittedGuiInt;
    int guiInt;

    private void OnGUI()
    {
        //basic button
        if (EzGui.Button("Button", 0, 0)) {
            Debug.Log("Gui button pressed");
        }

        //basic labels
        EzGui.Label("Label 1", 1, 0);
        EzGui.Label("Label 2", 0, 1);
        EzGui.Label("Label 3", 1, 1);

        //Fitted to text
        EzGui.FittedLabel("Very Very Long Label 3", 0, 2);

        //Snapped to corners (defaults to UpperLeft)
        EzGui.Label("upper right", 0, 0, EzCorner.UpperRight);
        EzGui.Label("lower left", 0, 0, EzCorner.LowerLeft);
        EzGui.Label("lower right", 0, 0, EzCorner.LowerRight);

        //Recolored and snapped to corner
        EzGui.Label("lower left", 1, 0, EzCorner.LowerLeft, EzColor.Important);
        EzGui.Label("lower left", 2, 0, EzCorner.LowerLeft, EzColor.True);
        EzGui.Label("lower left", 3, 0, EzCorner.LowerLeft, EzColor.False);
        EzGui.Label("lower left", 4, 0, EzCorner.LowerLeft, EzColor.Disabled);

        //Fields snapped to upper right corner
        guiText = EzGui.TextField(guiText, 1, 0, EzCorner.UpperRight);
        guiInt = EzGui.IntField(guiInt, 2, 0, EzCorner.UpperRight);
        guiFloat = EzGui.FloatField(guiFloat, 3, 0, EzCorner.UpperRight);

        //All EzGui methods have "Fitted" versions that are fit to the text.
        fittedGuiFloat = EzGui.FittedFloatField(fittedGuiFloat, 5, 0, EzCorner.LowerLeft);
        fittedGuiInt = EzGui.FittedIntField(fittedGuiInt, 6, 0, EzCorner.LowerLeft);
        EzGui.FittedLabel("Fitted Label", 2, 0);
        if (EzGui.FittedButton("Fit", 3, 0)) { }
    }
}

