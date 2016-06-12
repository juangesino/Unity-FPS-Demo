using UnityEngine;

// Use Networking methods.
using UnityEngine.Networking;

// Require the WeaponManager.
[RequireComponent(typeof (WeaponManager))]

// Inherit behaviour from the NetworkBehaviour.
public class PlayerShoot : NetworkBehaviour {

    // This is the tag that every player should have.
    // This is our way to know if what we shot is a player or not.
    private const string PLAYER_TAG = "Player";

    // TODO: Better comment.
    // Reference to the WeaponsGraphics.
    //[SerializeField]
    //private GameObject weaponGFX;

    // The ray is shot form the camera center, so we will need the camera.
    [SerializeField]
    private Camera cam;

    // Define what the shooting is allowed to hit.
    [SerializeField]
    private LayerMask mask;

    // We need a weapon to do the shooting.
    // We are going to get this weapon from the PlayerManager script.
    private PlayerWeapon currentWeapon;

    // Reference the WeaponManger.
    private WeaponManager weaponManager;
 
    // We need to do some checking.
    void Start ()
    {
        // We will check that the camera is referenced.
        if (cam == null)
        {
            // If the camera is not referenced, we will throw an exeception.
            Debug.LogError("PlayerShoot: No camera referenced!");
            // Disable this script.
            this.enabled = false;
        }

        // Set the WeaponGFX layer.
        //weaponGFX.layer = LayerMask.NameToLayer(weaponLayerName);

        // Instantiate the WeaponManager.
        weaponManager = GetComponent<WeaponManager>();
       
    }

    // Check shooting.
    void Update ()
    {
        // Get the current weapon from the WeaponManager.
        currentWeapon = weaponManager.GetCurrentWeapon();
        
        // If the weapon is semi-automatic.
        if (currentWeapon.fireRate <= 0f)
        {
            // If the player pressed the Fire button.
            if (Input.GetButtonDown("Fire1"))
            {
                // Call method to the the shooting.
                Shoot();
            }
        }
        else
        {
            // The weapon si full-auto!
            // If the player pressed the Fire button.
            if (Input.GetButtonDown("Fire1"))
            {
                // This will call the Shoot function, with a 0f delay and at (1/fireRate) intervals.
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp ("Fire1"))
            {
                // The player stopped pressing the fire button.
                CancelInvoke("Shoot");
            }
        }
    }

    // Shooting method.
    // This will be a Client method as it is only executed on the client, never on the server.
    [Client]
    void Shoot()
    {
        // We only want to cast the ray if we are the localPlayer.
        if (!isLocalPlayer)
        {
            return;
        }

        // Execute the shoot command.
        CmdOnShoot();

        // Store RaycastHit information in a _hit variable.
        RaycastHit _hit;

        // Now lets check if we hit anything.
        // cam.transform.position is the Ray origin.
        // cam.transform.forward is the Ray direction.
        // out _hit is where the Ray output will be.
        // currentWeapon.range is the Ray's maximum range.
        // mask is what the Ray is allow to hit.
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            // We hit something!
            // Everything we hit is stored in the _hit variable.
            // Now we'll check if what we hit is a Player.
            if (_hit.collider.tag == PLAYER_TAG)
            {
                // If it is a player, we want to let the server know.
                // Send the server the player's name and damage taken.
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            // Show a hit effect on the object we hit.
            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    // Now we need a Command method that WILL execute con the server.
    [Command]
    void CmdPlayerShot (string _PlayerID, int _damage)
    {
        // Let the server know that a player's been shot.
        // Lets find out what player has been shot using the GameManger GetPlayer method.
        Player _player = GameManager.GetPlayer(_PlayerID);

        // Now, lets damage the player. ¬‿¬
        _player.RpcTakeDamage(_damage);
    }

    // Another Command that executes on the server.
    // This command will execute everytime a player shoots.
    [Command]
    void CmdOnShoot ()
    {
        // Call a method on all the clients to display the MuzzleFlash.
        RpcDoShootEffect();
    }

    // Show the MuzzleFlash on every client.
    [ClientRpc]
    void RpcDoShootEffect ()
    {
        // Show the weapon's MuzzleFlash particles.
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }
    
    // This command will execute everytime a player hits something.
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        // Call a method on all the clients to display the HitParticles.
        RpcDoHitEffect(_pos, _normal);
    }

    // Show the HitEffect on every client.
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        // Show the weapon's HitEffect particles.
        // Use the given position and turn the normal into a Quaternion.
        // TODO: Object Pooling. (?)
        GameObject _hitEffect = (GameObject) Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));

        // Destroy the effect after 2 seconds.
        Destroy(_hitEffect, 2f);
    }

}
