using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_SceneSingletons : MonoBehaviour {
    static GameObject[] controllers;
    public static GameObject controllerL;
    public static GameObject controllerR;

    public static VRTK_ControllerEvents ControllerEventsL { get; set; }
    public static VRTK_ControllerEvents ControllerEventsR { get; set; }

    public static TextMesh debugText1;
    public static TextMesh debugText2;

    public static float WaterLevel = 0f;


    void Awake()
    {

        StartCoroutine(Init());

        //debugText1 = GameObject.FindGameObjectWithTag("DebugText1").GetComponent<TextMesh>();
        //debugText2 = GameObject.FindGameObjectWithTag("DebugText2").GetComponent<TextMesh>();
    }

    void Start () {
        //QualitySettings.vSyncCount = 0;

    }

    IEnumerator Init()
    {
        yield return new WaitUntil(TryFindControllers);
        InitControllers();
        InitControllerEvents();
    }

    static bool TryFindControllers()
    {
        controllers = GameObject.FindGameObjectsWithTag("GameController");
        if (controllers.Length == 2)
            return true;
        else
            return false;
    }

    void InitControllers()
    {

        controllerL = (controllers[0].name.Contains("left")) ? controllers[0] : controllers[1];
        controllerR = (controllers[1].name.Contains("right")) ? controllers[1] : controllers[0];


    }

    void InitControllerEvents()
    {
        ControllerEventsL = controllerL.GetComponent<VRTK_ControllerEvents>();
        ControllerEventsR = controllerR.GetComponent<VRTK_ControllerEvents>();
    }


}
