using System.Collections.Generic;
using UnityEngine;

public class EzGui : MonoBehaviour
{
    public static EzGui Instance;

    [Header("Spacing Options")]
    [Space(4)]
    public Vector2 Margin = new Vector2(42, 32);
    public Vector2 ItemSize = new Vector2(85 , 28);
    public Vector2 ItemSpacing = new Vector2(5, 5);

    [Space(16)]
    [Header("Button Color (Uses Alpha)")]
    [Space(4)]
    public Color BackgroundColor = new Color(.5f,.5f,.5f,.5f);

    [Space(16)]
    [Header("Text Options")]
    [Space(4)]
    public int FontSize = 11;
    public Color DefaultColor = Color.gray;
    public Color DisabledColor = Color.Lerp(Color.black, Color.gray, .5f);
    public Color TrueColor = Color.green;
    public Color FalseColor = Color.red;
    public Color ImportantColor = Color.yellow;
    [Space(16)]
    public GUISkin DefaultSkin;

    Dictionary<string, string> fieldStrings;

    private void Awake()
    {
        Instance = this;
        fieldStrings = new Dictionary<string, string>();
    }

    // Creates a label at a given row & column
    public static void Label(string labelText, float row, float column, EzColor colorState = EzColor.Default)
    {
        GUI.Label(GetRect(row, column), labelText, GetStyle(EzControlStyle.Label, colorState));
    }

    //Creates a button at a given row & column
    public static bool Button(string buttonText, float row, float column, EzColor colorState = EzColor.Default)
    {
        return (GUI.Button(GetRect(row, column), buttonText, GetStyle(EzControlStyle.Button, colorState)));
    }

    //Creates a text field at row & column
    public static string TextField(string fieldText, float row, float column, EzColor colorState = EzColor.Default)
    {
        return GUI.TextField(GetRect(row, column), (fieldText == null) ? "" : fieldText, GetStyle(EzControlStyle.Field, colorState));
    }

    //Creates an int field at row and column
    public static int IntField(int fieldInt, float row, float column, EzColor colorState = EzColor.Default)
    {
        return IntField(fieldInt, row, column, EzCorner.UpperLeft, colorState);
    }

    //Creates a float field at row and column
    public static float FloatField(float fieldFloat, float row, float column, EzColor colorState = EzColor.Default)
    {
        return FloatField(fieldFloat, row, column, EzCorner.UpperLeft, colorState);
    }

    #region "Fitted Versions"

    // Creates a label at a given row & column set to the size of the text
    public static void FittedLabel(string labelText, float row, float column, EzColor colorState = EzColor.Default)
    {
        GUI.Label(GetRect(labelText, row, column), labelText, GetStyle(EzControlStyle.Label, colorState));
    }

    //Creates a button at a given row & column set to the size of the text
    public static bool FittedButton(string buttonText, float row, float column, EzColor colorState = EzColor.Default)
    {
        GUILayout.Label(new GUIContent());
        return (GUI.Button(GetRect(buttonText, row, column), buttonText, GetStyle(EzControlStyle.Button, colorState)));
    }

    //Creates a text field at row & column
    public static string FittedTextField(string fieldText, float row, float column, EzColor colorState = EzColor.Default)
    {
        return GUI.TextField(GetRect(fieldText, row, column), (fieldText == null) ? "" : fieldText, GetStyle(EzControlStyle.Field, colorState));
    }

    //Creates an int field at row and column
    public static int FittedIntField(int fieldInt, float row, float column, EzColor colorState = EzColor.Default)
    {
        return FittedIntField(fieldInt, row, column, EzCorner.UpperLeft, colorState);
    }

    //Creates a float field at row and column
    public static float FittedFloatField(float fieldFloat, float row, float column, EzColor colorState = EzColor.Default)
    {
        return FittedFloatField(fieldFloat, row, column, EzCorner.UpperLeft, colorState);
    }
    #endregion

    #region "corner first overrides"

    public static void Label(string text, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        GUI.Label(GetRect(row, column, corner), text, GetStyle(EzControlStyle.Label, colorState));
    }

    public static bool FittedButton(string buttonText, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        return (GUI.Button(GetRect(buttonText, row, column, corner), buttonText, GetStyle(EzControlStyle.Button, colorState)));
    }

    public static void FittedLabel(string text, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        GUI.Label(GetRect(text, row, column, corner), text, GetStyle(EzControlStyle.Label, colorState));
    }

    public static bool Button(string text, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        return (GUI.Button(GetRect(row, column, corner), text, GetStyle(EzControlStyle.Button, colorState)));
    }

    public static string TextField(string fieldText, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        return GUI.TextField(GetRect(row, column, corner), (fieldText == null) ? "" : fieldText, GetStyle(EzControlStyle.Field, colorState));
    }
    
    public static string FittedTextField(string fieldText, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        return GUI.TextField(GetRect(fieldText, row, column, corner), (fieldText == null) ? "" : fieldText, GetStyle(EzControlStyle.Field, colorState));
    }
    
    public static int IntField(int fieldInt, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        string s = fieldInt.ToString();
        string key = GetKeyString(row, column, corner);
        if (CheckKey(key))
        {
            Instance.fieldStrings.TryGetValue(key, out s);
        }

        int result = 0;
        GUIStyle newStyle = int.TryParse(s, out result) ? GetStyle(EzControlStyle.Field, colorState) : GetStyle(EzControlStyle.Field, EzColor.False);
        s = GUI.TextField(GetRect(row, column, corner), s, newStyle);
        UpdateKey(key, s);

        if (int.TryParse(s, out result))
        {
            return result;
        }
        else
        {
            return fieldInt;
        }
    }

    public static float FloatField(float fieldFloat, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        string s = fieldFloat.ToString();
        string key = GetKeyString(row, column, corner);
        if (CheckKey(key))
        {
            Instance.fieldStrings.TryGetValue(key, out s);
        }

        float result = 0;
        GUIStyle newStyle = float.TryParse(s, out result) ? GetStyle(EzControlStyle.Field, colorState) : GetStyle(EzControlStyle.Field, EzColor.False);
        s = GUI.TextField(GetRect(row, column, corner), s, newStyle);
        UpdateKey(key, s);

        if (float.TryParse(s, out result))
        {
            return result;
        }
        else
        {
            return fieldFloat;
        }
    }
    
    public static int FittedIntField(int fieldInt, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        string s = fieldInt.ToString();
        string key = GetKeyString(row, column, corner);
        if (CheckKey(key))
        {
            Instance.fieldStrings.TryGetValue(key, out s);
        }

        int result = 0;
        GUIStyle newStyle = int.TryParse(s, out result) ? GetStyle(EzControlStyle.Field, colorState) : GetStyle(EzControlStyle.Field, EzColor.False);
        s = GUI.TextField(GetRect(s, row, column, corner), s, newStyle);
        UpdateKey(key, s);

        if (int.TryParse(s, out result))
        {
            return result;
        }
        else
        {
            return fieldInt;
        }
    }
    
    public static float FittedFloatField(float fieldFloat, float row, float column, EzCorner corner, EzColor colorState = EzColor.Default)
    {
        string s = fieldFloat.ToString();
        string key = GetKeyString(row, column, corner);
        if (CheckKey(key))
        {
            Instance.fieldStrings.TryGetValue(key, out s);
        }

        float result = 0;
        GUIStyle newStyle = float.TryParse(s, out result) ? GetStyle(EzControlStyle.Field, colorState) : GetStyle(EzControlStyle.Field, EzColor.False);
        s = GUI.TextField(GetRect(s, row, column, corner), s, newStyle);
        UpdateKey(key, s);

        if (float.TryParse(s, out result))
        {
            return result;
        }
        else
        {
            return fieldFloat;
        }
    }
    #endregion

    static void UpdateKey(string key, string value)
    {
        Instance.fieldStrings.Remove(key);
        Instance.fieldStrings.Add(key, value);
    }

    static bool CheckKey(string key)
    {
        return Instance.fieldStrings.ContainsKey(key);
    }

    static string GetKeyString(float row, float column, EzCorner corner)
    {
        return (int)corner + "|" + row + "" + column + "";
    }

    //Returns a GUIStyle object with the EzGui options applied
    public static GUIStyle GetStyle(EzControlStyle styleType, EzColor colorState = EzColor.Default)
    {
        EnsureExistence();
        GUIStyle newStyle;
        GUI.backgroundColor = Instance.BackgroundColor;
        if (Instance.DefaultSkin == null)
        {
            Instance.DefaultSkin = GUI.skin;
        }
        switch (styleType)
        {
            case EzControlStyle.Label:
            default:
                newStyle = Instance.DefaultSkin.label;
                break;
            case EzControlStyle.Button:
                newStyle = Instance.DefaultSkin.button;
                break;
            case EzControlStyle.Field:
                newStyle = Instance.DefaultSkin.textField;
                break;
        }
        SetColor(newStyle, colorState);
        newStyle.fontSize = Instance.FontSize;
        return newStyle;
    }

    // Converts a bool to an EzGui Textcolor for on / off labels
    public static EzColor BoolToTextColor(bool stateBool)
    {
        return (stateBool) ? EzColor.True : EzColor.False;
    }

    //Returns a rect object at a given row and column, defaulting to the upper left corner
    public static Rect GetRect(float row, float column, EzCorner corner = EzCorner.UpperLeft)
    {
        EnsureExistence();
        Vector2 rectXY = GetRectStart(row, column, Instance.ItemSize.x, Instance.ItemSize.y, corner);
        return new Rect(rectXY.x, rectXY.y, Instance.ItemSize.x, Instance.ItemSize.y);
    }

    //Returns a rect object at a given row and column sized to fit a string, defaulting to the upper left corner
    public static Rect GetRect(string elementContents, float row, float column, EzCorner corner = EzCorner.UpperLeft)
    {
        float xSize = Mathf.Max(10,Instance.DefaultSkin.label.CalcSize(new GUIContent(elementContents)).x);
        Vector2 rectXY = GetRectStart(row, column, xSize, Instance.ItemSize.y, corner);
        return new Rect(rectXY.x, rectXY.y, xSize, Instance.ItemSize.y);
    }

    //Ensures that the instance is not null
    public static void EnsureExistence()
    {
        if (Instance == null)
        {
            GameObject newInstance = new GameObject();
            newInstance.name = "EzGui";
            Instance = newInstance.AddComponent<EzGui>();
        }
    }

    #region "private methods"
    //Returns the top left corner of a rect wtih the given parameters
    private static Vector2 GetRectStart(float row, float column, float sizeX, float sizeY, EzCorner corner = EzCorner.UpperLeft)
    {
        Vector2 rectXY = Vector2.zero;
        switch (corner)
        {
            case EzCorner.UpperLeft:
                rectXY = new Vector2(Instance.Margin.x + column * (Instance.ItemSize.x + Instance.ItemSpacing.x), Instance.Margin.y + row * (sizeY + Instance.ItemSpacing.y));
                break;
            case EzCorner.LowerLeft:
                rectXY = new Vector2(Instance.Margin.x + column * (Instance.ItemSize.x + Instance.ItemSpacing.x), Screen.height - sizeY - Instance.Margin.y - row * (sizeY + Instance.ItemSpacing.y));
                break;
            case EzCorner.UpperRight:
                rectXY = new Vector2(Screen.width - sizeX - Instance.Margin.x - column * (Instance.ItemSize.x + Instance.ItemSpacing.x), Instance.Margin.y + row * (sizeY + Instance.ItemSpacing.y));
                break;
            case EzCorner.LowerRight:
                rectXY = new Vector2(Screen.width - sizeX - Instance.Margin.x - column * (Instance.ItemSize.x + Instance.ItemSpacing.x), Screen.height - sizeY - Instance.Margin.y - row * (sizeY + Instance.ItemSpacing.y));
                break;
        }
        return rectXY;
    }
    
    //Applies a color to the ui
    private static void SetColor(GUIStyle newStyle, EzColor colorState)
    {
        switch (colorState)
        {
            case EzColor.Disabled:
                newStyle.normal.textColor = Instance.DisabledColor;
                newStyle.hover.textColor = Instance.DisabledColor;
                newStyle.focused.textColor = Instance.DisabledColor;
                break;
            case EzColor.Default:
                newStyle.normal.textColor = Instance.DefaultColor;
                newStyle.hover.textColor = Instance.DefaultColor;
                newStyle.focused.textColor = Instance.DefaultColor;
                break;
            case EzColor.True:
                newStyle.normal.textColor = Instance.TrueColor;
                newStyle.hover.textColor = Instance.TrueColor;
                newStyle.focused.textColor = Instance.TrueColor;
                break;
            case EzColor.False:
                newStyle.normal.textColor = Instance.FalseColor;
                newStyle.hover.textColor = Instance.FalseColor;
                newStyle.focused.textColor = Instance.FalseColor;
                break;
            case EzColor.Important:
                newStyle.normal.textColor = Instance.ImportantColor;
                newStyle.hover.textColor = Instance.ImportantColor;
                newStyle.focused.textColor = Instance.ImportantColor;
                break;
        }
    }
    #endregion
}