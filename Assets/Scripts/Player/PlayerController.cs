using UnityEngine;

// We need a motor to take care of the movements.
[RequireComponent(typeof(PlayerMotor))]

// We need a ConfigurableJoint to handle spring behaviour.
[RequireComponent(typeof(ConfigurableJoint))]

// We are going to require the animator to change animation while moving.
[RequireComponent(typeof(Animator))]

public class PlayerController : MonoBehaviour {

    // Allows us to change the speed from Unity.
    [SerializeField]
    private float speed = 5f;

    // Allows us to change the rotation sensitivity from Unity.
    // This variable is similar to the speed but for rotation.
    [SerializeField]
    private float lookSensitivity = 3f;

    // This is going to be the default value for the thruster force.
    // This value can be changed from Unity.
    [SerializeField]
    private float thrusterForce = 1300f;

    // Speed at which fuel is consumed.
    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;

    // Speed at which fuel will be added.
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;

    // How much fuel we have.
    private float thrusterFuelAmount = 1f;

    // A layer for which players can hover on top of.
    [SerializeField]
    private LayerMask environmentMask;

    // Create a settings category for customizing the ConfigurableJoint settings.
    [Header("Spring Settings:")]
    // ConfigurableJoint spring value, default to 20.
    [SerializeField]
    private float jointSpring = 20f;

    // ConfigurableJoint max force value, default to 40.
    [SerializeField]
    private float jointMaxForce = 40f;

    // Variable that will store the motor.
    private PlayerMotor motor;

    // Variable that will store the ConfigurableJoint.
    private ConfigurableJoint joint;

    // Store the required animator in a variable.
    private Animator animator;

    // Create a getter method to get the thruster fuel.
    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    void Start ()
    {
        // Initialize the motor.
        motor = GetComponent<PlayerMotor>();

        // Initialize the ConfigurableJoint.
        joint = GetComponent<ConfigurableJoint>();

        // Initialize the Animator.
        animator = GetComponent<Animator>();

        // Configure the ConfigurableJoint with the values.
        SetJointSettings(jointSpring);
    }

    void Update ()
    {
        // We need to chenge the spring offset.
        // Right now, the player has an initial height of 1.
        // The spring will try to take back to that hight, even when we are on top of things.
        // We need to cast rays to check the object we are standing on heights,
        // And change the spring offset to that height plus 1.
        RaycastHit _hit;

        // Cast the ray at the players position, pointing down, store the output un _hit and with a range of 100f.
        // We also provide a LayerMask to define which layer this applies to.
        if (Physics.Raycast (transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            // If we hit something, change the joint's target position.
            // The y-axis has a negative sign because the offset has to be negative.
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            // If we didn't hit anything, set the targetPosition back to normal.
            joint.targetPosition = Vector3.zero;
        }

        // Calculate movement velocity as a 3D vector
        // First get the Horizontal input.
        // GetAxisRaw means that we don't want Unity to modify the input value.
        // We are going to use GetAxis so that Unity does some smoothing for us.
        // Horizontal is defined in the project input settings.
        float _xMove = Input.GetAxis("Horizontal");
        // Now lets get the Vertical input value.
        float _zMove = Input.GetAxis("Vertical");

        // We are going to build two Vector3 variables for each movement.
        // Horizontal movement Vector3 variable.
        Vector3 _movHorizontal = transform.right * _xMove;
        // Vertical movement Vector3 variable.
        Vector3 _movVertical = transform.forward * _zMove;

        // Final movement vector adding the vertical and horizontal movement.
        // .normalized turns the Vector3 into a unit vector (´versor´) so that it just represents a direction.
        // We can then multiply that direction by the speed to get a velocity vector.
        // TODO: Explain why we removed the .normalized.
        // Vector3 _velocity = (_movHorizontal + _movVertical).normalized * speed;
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

        // Animate movement. Set the ForwardVelocity blend.
        animator.SetFloat("ForwardVelocity", _zMove);

        // Apply movement using the motor.
        motor.Move(_velocity);

        // Calculate rotation as a Vector3.
        // This is only the X rotation because the Y rotation will be handled by the camera.
        float _yRotation = Input.GetAxisRaw("Mouse X");

        // We are using a lookSensitivity to modify the vector size (similar to speed).
        Vector3 _rotation = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        // Apply rotation.
        motor.Rotate(_rotation);


        // Calculate camera rotation as a float.
        // This is only the Y rotation handled by the camera.
        float _xRotation = Input.GetAxisRaw("Mouse Y");

        // We are using the same lookSensitivity as for player rotation.
        float _cameraRotationX = _xRotation * lookSensitivity;

        // Apply camera rotation.
        motor.RotateCamera(_cameraRotationX);

        // The default thruster force we are going to pass to the Motor is zero.
        Vector3 _thrusterForce = Vector3.zero;

        // If the Jump button has value, we are going to pass the thruster force to the Motor.
        // Of course only do this if we have fuel left!
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            // We jumped, so lets remove some fuel.
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            // Don't apply thruster until you have at least some fuel.
            if (thrusterFuelAmount >= 0.01f)
            {
                // We are using Vector3.up as the direction vector and apply the thrusterForce as the magnitude.
                _thrusterForce = Vector3.up * thrusterForce;

                // Disable the ConfigurableJoint spring value, set it to zero.
                SetJointSettings(0f);
            }
            
        } else
        {
            // If we are not using the thrusters, set the spring back to normal value.
            SetJointSettings(jointSpring);

            // If we are not using the thrusters, add some fuel back.
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;
        }

        // Make sure the thruster fuel does not exceed the limits.
        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        // Apply thruster force.
        motor.Thrust(_thrusterForce);
    }

    // Function to set ConfigurableJoint from variables.
    // This takes the jointSpring as an argument to allow us to modify it on runtime.
    private void SetJointSettings(float _jointSpring) {

        // The ConfigurableJoint has different structs that can be accessed as follows:
        // The struct we want to change is the yDrive (see ConfigurableJoint settings in Unit inspector).
        joint.yDrive = new JointDrive {
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce
        };

    }
}
