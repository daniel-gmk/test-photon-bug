using UnityEngine;

public class PlayerMovement : InheritedMigrationBehaviour
{
    public override void FixedUpdateNetwork()
    {
        var inputValid = GetInput(out GameplayInput input);
        Debug.Log("IS INPUT VALID?: " + inputValid);

        if (inputValid)
        {
            if (input.MoveDirection != Vector2.zero)
            {
                Debug.Log("$PlayerMovement: PLAYER AGENT IS DETECTING MOVEMENT: " + input.MoveDirection);
            }
            if (input.Buttons.IsSet(EInputButton.Jump))
            {
                Debug.Log("$PlayerMovement: PLAYER AGENT IS DETECTING JUMP BUTTON PRESS: " + input.Buttons.IsSet(EInputButton.Jump));
            }
        }
    }
}
