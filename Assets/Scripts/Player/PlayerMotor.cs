using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerMotor : MonoBehaviour {

    // Optional Camera component that can be rotated.
    [SerializeField]
    private Camera cam;

    // Set a rotation limit for the camera.
    // If not set, player will be able to rotate 360. Not cool.
    [SerializeField]
    private float cameraRotationLimit = 85f;

    // Default velocity for movement is zero.
    private Vector3 velocity = Vector3.zero;

    // Default rotation is zero.
    private Vector3 rotation = Vector3.zero;

    // Default camera rotationX is zero.
    private float cameraRotationX = 0f;

    // Default thruster force is zero.
    private Vector3 thrusterForce = Vector3.zero;

    // We need to store the current camera rotation to be able to cahnge it.
    private float currentCameraRotationX = 0f;

    // Player's Rigidbody component.
    private Rigidbody rb;

    // Initialize the Rigidbody.
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Public method for changing velocity from a Vector3 value.
    public void Move (Vector3 _velocity)
    {
        // velocity is the velocity used for moving.
        // _velocity is the new variable being passed.
        // Update the velocity value.
        velocity = _velocity;
    }

    // Public method for changing rotation from a Vector3 value.
    public void Rotate(Vector3 _rotation)
    {
        // rotation is the rotation used for turning.
        // _rotation is the new variable being passed.
        // Update the rotation value.
        rotation = _rotation;
    }

    // Public method for changing camera rotationX from a float value.
    public void RotateCamera(float _cameraRotationX)
    {
        // cameraRotationX is the rotation used for rotating the camera over the X axis.
        // _cameraRotationX is the new variable being passed.
        // Update the camereaRotationX value.
        cameraRotationX = _cameraRotationX;
    }

    // Public method for changing thruster force from a Vector3 value.
    public void Thrust(Vector3 _thrusterForce)
    {
        // thrusterForce is the thruster force used for thrustering.
        // _thrusterForce is the new variable being passed.
        // Update the thrusterForce value.
        thrusterForce = _thrusterForce;
    }

    // Run every physics iteration
    void FixedUpdate ()
    {
        // Move the object.
        PerformMovement();

        // Rotate the object.
        PerformRotation();

        // Rotate camera.
        PerformCameraRotation();

        // Apply thrusters.
        ApplyThrusters();
    }

    // Move based on velocity variable.
    void PerformMovement()
    {
        // Check if value is not default.
        if (velocity != Vector3.zero)
        {
            // Move the Rigidbody, adding velocity to the Rb's current position and multiply by the time.
            // .MovePosition takes care of the collision and other physics.
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    // Rotate based on rotation variable.
    void PerformRotation()
    {
        // Check if value is not default.
        if (rotation != Vector3.zero)
        {
            // Rotate the Rigidbody, multiplying the actual rotation by a Quaternion.
            // Because this methods need a Quaternion and what we have is a Vector3, we use .Euler to transform it.
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        }
    }

    // Rotate camera based on cameraRotationX variable
    void PerformCameraRotation()
    {
        // Check if value is not default and that the optional Camera is present.
        if (cameraRotationX != 0f && cam != null)
        {
            // We rotate the Camera component. We take the current rotation and change it with the cameraRotationX variable.
            // Use a minus sign to change the value from the input and invert controllers.
            // TODO: Make invert a boolean customizable option.
            currentCameraRotationX -= cameraRotationX;
            // We us the Clamp method to limit the rotation.
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
            // Apply the transform to the Camera component.
            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
        }
    }

    // Thrust based on thrust force variable.
    void ApplyThrusters ()
    {
        // Check if force is not zero.
        if (thrusterForce != Vector3.zero)
        {
            // The AddForce method applies a force to the Rigibody doing all physics calculations and simulations for us.
            // The AddFroce method requires a force and a ForceMode. The ForceMode should depend on the mass.
            // To allow changes in player mass, we are going to use ForceMode.Acceleration for a constant acceleration ForceMode (ignores mass).
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }
}
