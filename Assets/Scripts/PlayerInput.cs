using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

[RequireComponent (typeof(PlayerController))]
public class PlayerInput : MonoBehaviour {

    [Header ("Control Mapping")]
    public XboxButton jumpButton;
    public XboxAxis moveAxisX;
    public XboxAxis moveAxisY;
    public XboxAxis lookAxisX;
    public XboxAxis lookAxisY;

    [Header("Control Settings")]
    public bool invertLookX;
    public bool invertLookY;
    public bool invertMoveX;
    public bool invertMoveY;

    private XboxController controller;
    private PlayerController cont;

    private bool didQueryNumOfCtrlrs = false;

    private void Start()
    {
        cont = GetComponent<PlayerController>();
        Debug.Assert(cont != null, "Could not find PlayerController component");
        //Controller Queries
        if (!didQueryNumOfCtrlrs)
        {
            didQueryNumOfCtrlrs = true;

            int queriedNumberOfCtrlrs = XCI.GetNumPluggedCtrlrs();

            if (queriedNumberOfCtrlrs == 1)
            {
                Debug.Log("Only " + queriedNumberOfCtrlrs + " Xbox controller plugged in.");
            }
            else if (queriedNumberOfCtrlrs == 0)
            {
                Debug.Log("No Xbox controllers plugged in!");
            }
            else
            {
                Debug.Log(queriedNumberOfCtrlrs + " Xbox controllers plugged in.");
            }

            XCI.DEBUG_LogControllerNames();

            // This code only works on Windows
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Debug.Log("Windows Only:: Any Controller Plugged in: " + XCI.IsPluggedIn(XboxController.Any).ToString());

                Debug.Log("Windows Only:: Controller 1 Plugged in: " + XCI.IsPluggedIn(XboxController.First).ToString());
                Debug.Log("Windows Only:: Controller 2 Plugged in: " + XCI.IsPluggedIn(XboxController.Second).ToString());
                Debug.Log("Windows Only:: Controller 3 Plugged in: " + XCI.IsPluggedIn(XboxController.Third).ToString());
                Debug.Log("Windows Only:: Controller 4 Plugged in: " + XCI.IsPluggedIn(XboxController.Fourth).ToString());
            }
        }
    }

    private void Update()
    {
        //Register Inputs
        Vector2 moveAxis = new Vector2(XCI.GetAxis(moveAxisX), XCI.GetAxis(moveAxisY));

        Vector2 lookAxis = new Vector2(XCI.GetAxis(lookAxisX), XCI.GetAxis(lookAxisY));

        bool didJump = XCI.GetButtonDown(jumpButton);

        //Debug for keyboard
        #if UNITY_EDITOR_WIN
        if (!didJump) didJump = Input.GetKeyDown(KeyCode.Space);
        if (moveAxis.magnitude <= 0.1f)
        {
            moveAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        if (lookAxis.magnitude <= 0.1f)
        {
            lookAxis = new Vector2(Input.GetAxis("HorizontalArrow"), Input.GetAxis("VerticalArrow"));
        }
#endif

        //Invert
        lookAxis.x *= invertLookX ? -1f : 1f;
        lookAxis.y *= invertLookY ? -1f : 1f;
        moveAxis.x *= invertMoveX ? -1f : 1f;
        moveAxis.y *= invertMoveY ? -1f : 1f;

        //Send Inputs to PlayerController
        if (didJump) cont.Jump();
        cont.Move(moveAxis);
        cont.Look(lookAxis);
    }

}
