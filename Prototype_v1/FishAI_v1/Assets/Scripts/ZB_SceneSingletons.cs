using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_SceneSingletons : MonoBehaviour {
    static GameObject[] controllers;
    public static GameObject controllerL;
    public static GameObject controllerR;

    public static VRTK_ControllerEvents ControllerEventsL { get; set; }
    public static VRTK_ControllerEvents ControllerEventsR { get; set; }

    void Awake()
    {

        StartCoroutine(Init());
    }

    void Start () {

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
