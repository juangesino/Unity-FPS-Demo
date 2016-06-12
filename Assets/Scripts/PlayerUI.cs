using UnityEngine;

public class PlayerUI : MonoBehaviour {

    // Define a variable to store the current thruster fill.
    [SerializeField]
    RectTransform thrusterFuelFill;

    // Reference to the player controller.
    private PlayerController controller;

    // Set the PlayerController.
    public void SetController(PlayerController _controller)
    {
        controller = _controller;
    }

    // Method that changes the fill amount.
    void SetFuelAmount (float _amount)
    {
        // Scale the UI bar to the fuel fill.
        // We just scale the Y axis.
        thrusterFuelFill.localScale = new Vector3(1f, _amount, 1f);
    }

    void Update ()
    {
        // Set the fuel.
        SetFuelAmount (controller.GetThrusterFuelAmount());
    }

}
