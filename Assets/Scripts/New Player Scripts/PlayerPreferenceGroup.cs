using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Prefs Group")]
public class PlayerPreferenceGroup : ScriptableObject
{
    [Header("Input Type")]
    public ControlType ControlType = ControlType.KBM;

    [Header("Keyboard Input")]
    public KeyCode KeyMoveUp = KeyCode.W;
    public KeyCode KeyMoveLeft = KeyCode.A;
    public KeyCode KeyMoveDown = KeyCode.S;
    public KeyCode KeyMoveRight = KeyCode.D;

    public KeyCode KeyJump = KeyCode.Space;
    public KeyCode KeyCrouch = KeyCode.LeftControl;
    public KeyCode KeySprint = KeyCode.LeftShift;

    [Header("Mouse Input")]
    public string AxisMouseX = "Mouse X";
    public string AxisMouseY = "Mouse Y";
    public float MouseSensitivity = 1f;
    public bool LockCursor = true;
}
public enum ControlType
{
    KBM
}