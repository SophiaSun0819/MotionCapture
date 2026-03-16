using UnityEngine;
using UnityEngine.XR;

public class SwitchView : MonoBehaviour
{
    public Transform xrOrigin;

    public Transform outsidePoint;
    public Transform insidePoint;

    public Transform character;

    bool isOutside = true;
    bool lastState = false;

    void Update()
    {
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool bButton;
        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bButton))
        {
            if (bButton && !lastState)
            {
                ToggleView();
            }

            lastState = bButton;
        }
    }

    void ToggleView()
    {
        isOutside = !isOutside;

        if (isOutside)
        {
            // Outside 视角
            xrOrigin.position = outsidePoint.position;
            xrOrigin.rotation = outsidePoint.rotation;

            // Character 变回平级
            character.SetParent(null, true);
        }
        else
        {
            // Inside 视角
            xrOrigin.position = insidePoint.position;
            xrOrigin.rotation = insidePoint.rotation;

            // Character 成为 XR Origin 子物体
            character.SetParent(xrOrigin, true);
        }
    }
}