using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WowIntMono_XInputGamepadInteger : MonoBehaviour
{

    public UnityEvent<int> m_onIntegerPushed;

    public XInputGamepad m_gamepad = new XInputGamepad();
    [System.Serializable]
    public class XInputGamepad{
        public bool m_buttonA = false;
        public bool m_buttonX = false;
        public bool m_buttonB = false;
        public bool m_buttonY = false;
        public bool m_sideButtonLeft = false;  
        public bool m_sideButtonRight = false;
        public bool m_joystickLeftButton = false;
        public bool m_joystickRightButton = false;
        public bool m_buttonMenuLeft = false;
        public bool m_buttonMenuRight = false;
        public bool m_buttonMenuCenter = false;
        public float m_triggerLeft = 0;
        public float m_triggerRight = 0;
        public Vector2 m_joystickLeft = Vector2.zero;
        public Vector2 m_joystickRight = Vector2.zero;
        public Vector2 m_arrows= Vector2.zero;
    }

    public int m_buttonAsInteger = 0;
    private int m_buttonAsIntegerPrevious = 0;
    public string m_buttonAsIntegerAs1And0 = "00000000 00000000 00000000 00000000";
    public int m_joystickAsInteger = 0;
    private int m_joystickAsIntegerPrevious = 0;

    public int m_lastPushed;

    public void OnValidate()
    {
        PushAxisAndButtons();
    }

    public void PushAxisAndButtons() {
        PushGameStateAsInteger();
        PushJoystickAsInteger();

    }
    public void PushGameStateAsInteger() {

        int value = 0;
    
        SetBitInInteger(value, 0, m_gamepad.m_buttonA, out value);
        SetBitInInteger(value, 1, m_gamepad.m_buttonX, out value);
        SetBitInInteger(value, 2, m_gamepad.m_buttonB, out value);
        SetBitInInteger(value, 3, m_gamepad.m_buttonY, out value);
        SetBitInInteger(value, 4, m_gamepad.m_sideButtonLeft, out value);
        SetBitInInteger(value, 5, m_gamepad.m_sideButtonRight, out value);
        SetBitInInteger(value, 6, m_gamepad.m_joystickLeftButton, out value);
        SetBitInInteger(value, 7, m_gamepad.m_joystickRightButton, out value);
        SetBitInInteger(value, 8, m_gamepad.m_buttonMenuLeft, out value);
        SetBitInInteger(value, 9, m_gamepad.m_buttonMenuRight, out value);
        SetBitInInteger(value, 10, m_gamepad.m_buttonMenuCenter, out value);
        SetBitInInteger(value, 11, m_gamepad.m_arrows.y>0.2f, out value);
        SetBitInInteger(value, 12, m_gamepad.m_arrows.x>0.2f, out value);
        SetBitInInteger(value, 13, m_gamepad.m_arrows.y<-0.2f, out value);
        SetBitInInteger(value, 14, m_gamepad.m_arrows.x<-0.2f, out value);
        SetBitInInteger(value, 18, m_gamepad.m_triggerLeft>=0.25, out value);
        SetBitInInteger(value, 19, m_gamepad.m_triggerLeft >= 0.5, out value);
        SetBitInInteger(value, 20, m_gamepad.m_triggerLeft>=0.75, out value);
        SetBitInInteger(value, 21, m_gamepad.m_triggerRight>=0.25, out value);
        SetBitInInteger(value, 22, m_gamepad.m_triggerRight >= 0.5, out value);
        SetBitInInteger(value, 23, m_gamepad.m_triggerRight>=0.75, out value);

        value += 1700000000;
        m_buttonAsInteger = value;
        m_buttonAsIntegerAs1And0= TurnInToIntegerRepresentation(m_buttonAsInteger - 1700000000);
        if(m_buttonAsInteger != m_buttonAsIntegerPrevious)
        {
            m_buttonAsIntegerPrevious = m_buttonAsInteger;
            m_lastPushed= m_buttonAsInteger;
            m_onIntegerPushed.Invoke(m_buttonAsInteger);
        }
    }



    private string TurnInToIntegerRepresentation(int value )
    {
        string result = "";
        for (int i = 31; i >= 0; i--)
        {
            result += (value >> i & 1) == 1 ? "1" : "0";
            if (i % 8 == 0)
                result += " ";
        }
        return result;
    }

    public void PushJoystickAsInteger()
    {

        int value = 0;
        value += TurnPercentFrom1To99(m_gamepad.m_joystickRight.y) * 1;
        value += TurnPercentFrom1To99(m_gamepad.m_joystickRight.x) * 100;
        value += TurnPercentFrom1To99(m_gamepad.m_joystickLeft.y) *  10000;
        value += TurnPercentFrom1To99(m_gamepad.m_joystickLeft.x) *  1000000;
        value += 1800000000;
        m_joystickAsInteger = value;
        if (m_joystickAsInteger != m_joystickAsIntegerPrevious)
        {
            m_joystickAsIntegerPrevious = m_joystickAsInteger;
            m_lastPushed = m_joystickAsInteger;
            m_onIntegerPushed.Invoke(m_joystickAsInteger);
        }
    }
    public int  TurnPercentFrom1To99(float value) { 

        if(value == 0)
            return  0;
        else
            return  (int)((((Mathf.Clamp(value,-1f,1f)+1f)/2) * 98f) + 1f);
    }

    public void SetBitInInteger(int value, int index0to31, bool isBitTrue, out int newValue) { 
        int mask = 1 << index0to31;
        if (isBitTrue) {
            newValue = value | mask;
        } 
        else {
            newValue = value & ~mask;
        }     
    }
    //111 = 7 -1 -0.8 -0.5 0 0.5 0.8 1
    //1111 = 15  1/7=0.15  -1 -0.85 -0.7 -0.55 -0.4 -0.25 -0.1 0 0.1 0.25 0.4 0.55 0.7 0.85 1
    //1111 11111 11111 11111 11111 11111
    //111111111 1111111111 1111111111 6 axis in 24 bits
    //111 111 111 111 111 111 6 axis in 18 bits
    //111,111,11 1,111,111,1 11,111111 Giving 6 bits as possible buttons
    //I suppose A X B Y SBL SBR

    //11 = 3 -1 0 1   111 as arrow pad
    //11,11,11,11 11,11|111|1 11111111
    //LH LV RH RV JL,JR DPAD, Back A X B Y SBL SBR JBL JBR  

    public int m_randomizedAll = 1399;
    public int m_intReleaseAll = 2399;
    public int m_hardwareJoystickPress = 1390;
    public int m_hardwareJoystickRelease = 2390;
    public int m_pressXboxButton = 1319;
    public int m_releaseXboxButton = 2319;
    public int m_randomizeAxis = 1320;
    public int m_releaseAxis = 2320;
    public int m_startRecording = 1321;
    public int m_stopRecording = 2321;

    public void PushInteger(int value) { 
    
        m_lastPushed = value;
        m_onIntegerPushed.Invoke(value);
    }

    public void ReleaseAll()=>PushInteger(m_intReleaseAll);
    public void RandomizeAll() => PushInteger(m_randomizedAll);
    public void HardwareJoystickPress() => PushInteger(m_hardwareJoystickPress);
    public void HardwareJoystickRelease() => PushInteger(m_hardwareJoystickRelease);
    public void PressXboxButton() => PushInteger(m_pressXboxButton);
    public void ReleaseXboxButton() => PushInteger(m_releaseXboxButton);
    public void RandomizeAxis() => PushInteger(m_randomizeAxis);
    public void ReleaseAxis() => PushInteger(m_releaseAxis);
    public void StartRecording() => PushInteger(m_startRecording);
    public void StopRecording() => PushInteger(m_stopRecording);



    public void SetKeyA(bool isPress) { 
    
        if(isPress)
            PressA();
        else
            ReleaseA();
    }
    public void SetKeyX(bool isPress) { 
    
        if(isPress)
            PressX();
        else
            ReleaseX();
    }
    public void SetKeyB(bool isPress) { 
    
        if(isPress)
            PressB();
        else
            ReleaseB();
    }
    public void SetKeyY(bool isPress) { 
    
        if(isPress)
            PressY();
        else
            ReleaseY();
    }
    public void SetKeySideButtonLeft(bool isPress) { 
    
        if(isPress)
            PressSideButtonLeft();
        else
            ReleaseSideButtonLeft();
    }
    public void SetKeySideButtonRight(bool isPress) { 
    
        if(isPress)
            PressSideButtonRight();
        else
            ReleaseSideButtonRight();
    }
    public void SetKeyJoystickLeftButton(bool isPress)
    {

        if (isPress)
            PressJoystickLeftButton();
        else
            ReleaseJoystickLeftButton();
    }
    public void SetKeyJoystickRightButton(bool isPress)
    {

        if (isPress)
            PressJoystickRightButton();
        else
            ReleaseJoystickRightButton();
    }
    public void SetKeyMenuLeft(bool isPress)
    {

        if (isPress)
            PressMenuLeft();
        else
            ReleaseMenuLeft();
    }

    public void SetKeyMenuRight(bool isPress)
    {

        if (isPress)
            PressMenuRight();
        else
            ReleaseMenuRight();
    }
    public void SetKeyMenuCenter(bool isPress)
    {

        if (isPress)
            PressMenuCenter();
        else
            ReleaseMenuCenter();
    }


    public void SetTriggerLeftAsPress(bool isPress)
        {
        if (isPress)
            SetTriggerLeft(1);
        else
            SetTriggerLeft(0);
    }
    public void SetTriggerRightAsPress(bool isPress)
    {
        if (isPress)
            SetTriggerRight(1);
        else
            SetTriggerRight(0);
    }


    public void PressA()
    {
        m_gamepad.m_buttonA = true;
        PushAxisAndButtons();
    }
    public void ReleaseA()
    {
        m_gamepad.m_buttonA = false;
        PushAxisAndButtons();
    }
    public void PressX()
    {
        m_gamepad.m_buttonX = true;
        PushAxisAndButtons();
    }
    public void ReleaseX()
    {
        m_gamepad.m_buttonX = false;
        PushAxisAndButtons();
    }
    public void PressB()
    {
        m_gamepad.m_buttonB = true;
        PushAxisAndButtons();
    }
    public void ReleaseB()
    {
        m_gamepad.m_buttonB = false;
        PushAxisAndButtons();
    }
    public void PressY()
    {
        m_gamepad.m_buttonY = true;
        PushAxisAndButtons();
    }
    public void ReleaseY()
    {
        m_gamepad.m_buttonY = false;
        PushAxisAndButtons();
    }
    public void PressSideButtonLeft()
    {
        m_gamepad.m_sideButtonLeft = true;
        PushAxisAndButtons();
    }
    public void ReleaseSideButtonLeft()
    {
        m_gamepad.m_sideButtonLeft = false;
        PushAxisAndButtons();
    }
    public void PressSideButtonRight()
    {
        m_gamepad.m_sideButtonRight = true;
        PushAxisAndButtons();
    }
    public void ReleaseSideButtonRight()
    {
        m_gamepad.m_sideButtonRight = false;
        PushAxisAndButtons();
    }
    public void PressJoystickLeftButton()
    {
        m_gamepad.m_joystickLeftButton = true;
        PushAxisAndButtons();
    }
    public void ReleaseJoystickLeftButton()
    {
        m_gamepad.m_joystickLeftButton = false;
        PushAxisAndButtons();
    }
    public void PressJoystickRightButton()
    {
        m_gamepad.m_joystickRightButton = true;
        PushAxisAndButtons();
    }
    public void ReleaseJoystickRightButton()
    {
        m_gamepad.m_joystickRightButton = false;
        PushAxisAndButtons();
    }
    public void PressMenuLeft()
    {
        m_gamepad.m_buttonMenuLeft = true;
        PushAxisAndButtons();
    }
    public void ReleaseMenuLeft()
    {
        m_gamepad.m_buttonMenuLeft = false;
        PushAxisAndButtons();
    }
    public void PressMenuRight()
    {
        m_gamepad.m_buttonMenuRight = true;
        PushAxisAndButtons();
    }
    public void ReleaseMenuRight()
    {
        m_gamepad.m_buttonMenuRight = false;
        PushAxisAndButtons();
    }
    public void PressMenuCenter()
    {
        m_gamepad.m_buttonMenuCenter = true;
        PushAxisAndButtons();
    }
    public void ReleaseMenuCenter()
    {
        m_gamepad.m_buttonMenuCenter = false;
        PushAxisAndButtons();
    }
    public void SetTriggerLeft(float value)
    {
        m_gamepad.m_triggerLeft = value;
        PushAxisAndButtons();
    }
    public void SetTriggerRight(float value)
    {
        m_gamepad.m_triggerRight = value;
        PushAxisAndButtons();
    }
    public void SetJoystickLeft(Vector2 value)
    {
        m_gamepad.m_joystickLeft = value;
        PushAxisAndButtons();
    }
    public void SetJoystickRight(Vector2 value)
    {
        m_gamepad.m_joystickRight = value;
        PushAxisAndButtons();
    }
    public void SetArrows(Vector2 value)
    {
        m_gamepad.m_arrows = value;
        PushAxisAndButtons();
    }
    public void SetArrowsX(float value)
    {
        m_gamepad.m_arrows.x = value;
        PushAxisAndButtons();
    }
    public void SetArrowsY(float value)
    {
        m_gamepad.m_arrows.y = value;
        PushAxisAndButtons();
    }
    public void SetArrowsUp()
    {
        m_gamepad.m_arrows.y = 1;
        PushAxisAndButtons();
    }
    public void SetArrowsDown()
    {
        m_gamepad.m_arrows.y = -1;
        PushAxisAndButtons();
    }
    public void SetArrowsLeft()
    {
        m_gamepad.m_arrows.x = -1;
        PushAxisAndButtons();
    }
    public void SetArrowsRight()
    {
        m_gamepad.m_arrows.x = 1;
        PushAxisAndButtons();
    }
    public void SetArrowsCenter()
    {
        m_gamepad.m_arrows = Vector2.zero;
        PushAxisAndButtons();
    }

    public void SetArrowsN()
    {
        m_gamepad.m_arrows.y = 1;
        m_gamepad.m_arrows.x = 0;
        PushAxisAndButtons();
    }
    public void SetArrowsNE()
    {
        m_gamepad.m_arrows.y = 1;
        m_gamepad.m_arrows.x = 1;
        PushAxisAndButtons();
    }
    public void SetArrowsE()
    {
        m_gamepad.m_arrows.y = 0;
        m_gamepad.m_arrows.x = 1;
        PushAxisAndButtons();
    }
    public void SetArrowsSE()
    {
        m_gamepad.m_arrows.y = -1;
        m_gamepad.m_arrows.x = 1;
        PushAxisAndButtons();
    }
    public void SetArrowsS()
    {
        m_gamepad.m_arrows.y = -1;
        m_gamepad.m_arrows.x = 0;
        PushAxisAndButtons();
    }
    public void SetArrowsSW()
    {
        m_gamepad.m_arrows.y = -1;
        m_gamepad.m_arrows.x = -1;
        PushAxisAndButtons();
    }
    public void SetArrowsW()
    {
        m_gamepad.m_arrows.y = 0;
        m_gamepad.m_arrows.x = -1;
        PushAxisAndButtons();
    }
    public void SetArrowsNW()
    {
        m_gamepad.m_arrows.y = 1;
        m_gamepad.m_arrows.x = -1;
        PushAxisAndButtons();
    }


    public void SetJoystickLeftUp()
    {
        m_gamepad.m_joystickLeft = Vector2.up;
        PushAxisAndButtons();
    }
    public void SetJoystickLeftDown()
    {
        m_gamepad.m_joystickLeft = Vector2.down;
        PushAxisAndButtons();
    }
    public void SetJoystickLeftLeft()
    {
        m_gamepad.m_joystickLeft = Vector2.left;
        PushAxisAndButtons();
    }
    public void SetJoystickLeftRight()
    {
        m_gamepad.m_joystickLeft = Vector2.right;
        PushAxisAndButtons();
    }
    public void SetJoystickLeftCenter()
    {
        m_gamepad.m_joystickLeft = Vector2.zero;
        PushAxisAndButtons();
    }
    public void SetJoystickRightUp()
    {
        m_gamepad.m_joystickRight = Vector2.up;
        PushAxisAndButtons();
    }
    public void SetJoystickRightDown()
    {
        m_gamepad.m_joystickRight = Vector2.down;
        PushAxisAndButtons();
    }
    public void SetJoystickRightLeft()
    {
        m_gamepad.m_joystickRight = Vector2.left;
        PushAxisAndButtons();
    }
    public void SetJoystickRightRight()
    {
        m_gamepad.m_joystickRight = Vector2.right;
        PushAxisAndButtons();
    }
    public void SetJoystickRightCenter()
    {
        m_gamepad.m_joystickRight = Vector2.zero;
        PushAxisAndButtons();
    }
    public void SetJoystickLeft(float x, float y)
    {
        m_gamepad.m_joystickLeft = new Vector2(x, y);
        PushAxisAndButtons();
    }
    public void SetJoystickRight(float x, float y)
    {
        m_gamepad.m_joystickRight = new Vector2(x, y);
        PushAxisAndButtons();
    }
    public void SetJoystickLeftHorizontal(float x)
    {
        m_gamepad.m_joystickLeft = new Vector2(x, m_gamepad.m_joystickLeft.y);
        PushAxisAndButtons();
    }
    public void SetJoystickLeftVertical(float y)
    {
        m_gamepad.m_joystickLeft = new Vector2(m_gamepad.m_joystickLeft.x, y);
        PushAxisAndButtons();
    }
    public void SetJoystickRightHorizontal(float x)
    {
        m_gamepad.m_joystickRight = new Vector2(x, m_gamepad.m_joystickRight.y);
        PushAxisAndButtons();
    }
    public void SetJoystickRightVertical(float y)
    {
        m_gamepad.m_joystickRight = new Vector2(m_gamepad.m_joystickRight.x, y);
        PushAxisAndButtons();
    }
    
}
