using UnityEngine;

// This will allow us to change values on Unity inspector.
[System.Serializable]

// This is just a normal class with no MonoBehaviour inheritance.
public class PlayerWeapon {

    // We need to store some variables for our weapon.
    // Store a name.
    public string name = "Glock";

    // Set a value for the weapon's damage.
    public int damage = 10;

    // Set a value for the weapon's range.
    public float range = 100f;

    // Fire rate. 0 means semi-automatic. Above 0 means full auto.
    public float fireRate = 0f;

    // Give the weapon some shiny graphics!
    public GameObject graphics;

}
