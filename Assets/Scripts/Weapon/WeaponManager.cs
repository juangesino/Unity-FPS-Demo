using UnityEngine;

// Call the network api.
using UnityEngine.Networking;

// Inherit NetworkBehaviour.
public class WeaponManager : NetworkBehaviour {

    // Store the Weapon layer name;
    [SerializeField]
    private string weaponLayerName = "Weapon";

    // A place where the weapon should be instantiated.
    [SerializeField]
    private Transform weaponHolder;

    // We are going to equip the player with a default weapon.
    [SerializeField]
    private PlayerWeapon primaryWeapon;

    // We need to store which weapon is equiped currently.
    private PlayerWeapon currentWeapon;

    // Keep track of the current weapon's graphics.
    private WeaponGraphics currentGraphics;

    // At the begining we just use the default weapon.
    void Start ()
    {
        // Call the method to change the currentWeapon.
        EquipWeapon (primaryWeapon);
    }

    // Method to change the currentWeapon.
    void EquipWeapon(PlayerWeapon _weapon)
    {
        // Change the currentWeapon.
        currentWeapon = _weapon;

        // Instatiate the graphics for this weapon.
        GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);

        // Make it a child of the WeaponHolder.
        _weaponIns.transform.SetParent(weaponHolder);

        // Find the weapon's graphics and store it.
        currentGraphics = _weaponIns.GetComponent<WeaponGraphics>();

        // Check that the weapon we are using actually has graphics.
        if (currentWeapon == null)
        {
            // Throw error.
            Debug.LogError("WeaponManager: No WeaponGraphics component on the weapon object (" + _weaponIns.name + ").");
        }

        // LocalPlayer's weapon has a special LayerMask.
        if (isLocalPlayer)
        {
            // Set the layer name recursively.
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));
        }
    }

    // Public method to get the current weapon.
    public PlayerWeapon GetCurrentWeapon()
    {
        // Just return the currentWeapon.
        return currentWeapon;
    }

    // Public method to get the current weapon's graphics.
    public WeaponGraphics GetCurrentGraphics()
    {
        // Just return the currentGraphics.
        return currentGraphics;
    }

}
