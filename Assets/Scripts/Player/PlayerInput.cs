using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KBMInputGroup : IPlayerInput
{
    #region Private Vars
    private readonly KeyCode moveUp;
    private readonly KeyCode moveLeft;
    private readonly KeyCode moveDown;
    private readonly KeyCode moveRight;

    private readonly KeyCode inputJump;
    private readonly KeyCode inputSprint;
    private readonly KeyCode inputCrouch;

    private readonly string mouseX;
    private readonly string mouseY;
    private readonly float mouseSensitivity;
    #endregion

    #region Constructor
    public KBMInputGroup(PlayerPreferenceGroup prefs)
    {
        // Assign motion keys
        moveUp = prefs.KeyMoveUp;
        moveLeft = prefs.KeyMoveLeft;
        moveDown = prefs.KeyMoveDown;
        moveRight = prefs.KeyMoveRight;

        // Assign action keys
        inputJump = prefs.KeyJump;
        inputSprint = prefs.KeySprint;
        inputCrouch = prefs.KeyCrouch;

        // Assign mouse options
        mouseX = prefs.AxisMouseX;
        mouseY = prefs.AxisMouseY;
        mouseSensitivity = prefs.MouseSensitivity;

        // Apply cursor lock
        if (prefs.LockCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
    }
    #endregion
    
    #region Methods
    public Vector2 GetAxisMotion()
    {
        float hor = 0f;
        float vert = 0f;

        if (Input.GetKey(moveLeft)) hor -= 1;
        if (Input.GetKey(moveRight)) hor += 1;
        if (Input.GetKey(moveUp)) vert += 1;
        if (Input.GetKey(moveDown)) vert -= 1;

        // Clamping magnitude instead or normalizing as the code i'm basing mine
        // off of is said to break with normalization
        return Vector2.ClampMagnitude(new Vector2(hor, vert), 1f);
    }
    public Vector2 GetAxisLook()
    {
        float hor = Input.GetAxis(mouseX);
        float vert = Input.GetAxis(mouseY);

        return new Vector2(hor, vert) * mouseSensitivity;
    }

    public bool GetInputSprint() => Input.GetKey(inputSprint);
    public bool GetInputCrouch() => Input.GetKey(inputCrouch);
    public bool GetInputJump() => Input.GetKeyDown(inputJump);
    #endregion
}
public interface IPlayerInput
{
    Vector2 GetAxisMotion();
    Vector2 GetAxisLook();

    bool GetInputSprint();
    bool GetInputCrouch();
    bool GetInputJump();
}