using UnityEngine;

// Network API.
using UnityEngine.Networking;

// Collections.
using System.Collections;

// Make sure we have a PlayerSetup.
[RequireComponent(typeof(PlayerSetup))]

// Inherit NetworkBehaviour.
public class Player : NetworkBehaviour {

    // Boolean variable that stores if a player is dead or not.
    // This variable must sync accross network.
    [SyncVar]
    private bool _isDead = false;

    // Define a public getter and a private setter for the variable.
    public bool isDead
    {
        // The public getter.
        get { return _isDead;  }
        // The private or protected setter.
        protected set { _isDead = value; }
    }

    // The player's maximum health.
    [SerializeField]
    private int maxHealth = 100;

    // The player's current health.
    // This needs to be synced with all players.
    [SyncVar]
    private int currentHealth;

    // We are going to store all components that should be disabled when a player dies inside an array.
    [SerializeField]
    private Behaviour[] disableOnDeath;
    // We need to know what compoentns were enabled.
    private bool[] wasEnabled;

    // Store GameObjects to be disabled on death.
    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    // Reference to the death explosion.
    [SerializeField]
    private GameObject deathEffect;

    // Reference to the spawn effect.
    [SerializeField]
    private GameObject spawnEffect;

    // We only want to find the enabled components when we first setup the player.
    // Lets keep track of that.
    private bool firstSetup = true;

    // The awake function runs before the start.
    // This WAS an Awake function.
    // It needs to be another function because we need this to run after the PlayerSetup.
    // So we changed it to a public function and called it from PlayerSetup.
    public void SetupPlayer ()
    {
        // Sitch cameras and enable the UI is we are the local player.
        if (isLocalPlayer)
        {
            // Switch cameras.
            GameManager.singleton.SetSceneCameraActive(false);

            // Enable the UI.
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

        // Tell the server to tell the clients to setup the new player.
        CmdBroadcastNewPlayerSetup();
    }

    // Tell all clients that a new player should be setup.
    [Command]
    private void CmdBroadcastNewPlayerSetup ()
    {
        // Tell the clients to setup the new player.
        RpcSetupPlayerOnAllClients();
    }

    // Client should do the setup for the new player.
    [ClientRpc]
    private void RpcSetupPlayerOnAllClients ()
    {
        // Only run this if the player is beign setup for the first time.
        // TODO: This is a bit hacky. Change.
        if (firstSetup)
        {
            // We need to setup the wasEnabled boolean array to know if the components were enabled or not.
            // Create the array with same lenght as the amount of components we want to disable on death.
            wasEnabled = new bool[disableOnDeath.Length];

            // Loop the array we just created to store if components are enabled or not.
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                // Set everything to the current enabled state.
                wasEnabled[i] = disableOnDeath[i].enabled;
            }

            // Now that we have setup the player, change this to false.
            firstSetup = false;
        }

        // Set all starting defaults.
        SetDefaults();
    }

    // This method will set all variables to default game values.
    public void SetDefaults()
    {
        // Make the player start alive.
        isDead = false;

        // Set the current health to maximum.
        currentHealth = maxHealth;

        // Set components to whatever state they had.
        // Iterate the disabledOnDeath array.
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            // Set the components to the enabled state we stored.
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        // Because Colliders are not Behaviours, we need to disable/enable them separately.
        // Set the Coolider.
        Collider _col = GetComponent<Collider>();

        // We need to make sure that the collider is present.
        if (_col != null)
        {
            // If we have a Collider, enable it.
            _col.enabled = true;
        }

        // Enable GameObjects for that player.
        // Loop the GameObjects we need to enable.
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            // Enable those GameObjects.
            disableGameObjectsOnDeath[i].SetActive(true);
        }
        
        // Show spawn effect at player's new position (we don't care about rotation).
        GameObject _gfxIns = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);

        // Destroy the effects after 3 seconds.
        Destroy(_gfxIns, 3f);

    }

    // Method to inflict damage and lower the currentHealth by a certain amount.
    // Because we want to do some dying and respawning, we want all clients to execute this.
    // The ClientRpc makes sure the method executes on all clients. It's like [SyncVar] but for methods.
    [ClientRpc]
    public void RpcTakeDamage (int _amount)
    {
        // Don't damage if you are dead.
        if (_isDead) { return; }

        // Subtract the amount given to the current health.
        currentHealth -= _amount;

        // TODO: Remove debug.
        Debug.Log(transform.name + " now has " + currentHealth + " health.");

        // Check if player is now dead.
        if (currentHealth <= 0)
        {
            // Kill it.
            Die();
        }
    }

    private void Die ()
    {
        isDead = true;

        // Disable components for that player.
        // Loop the components we need to disable.
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            // Disable those components.
            disableOnDeath[i].enabled = false;
        }

        // Disable GameObjects for that player.
        // Loop the GameObjects we need to disable.
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            // Disable those GameObjects.
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        // Disable the collider.
        // As mentioned in the SetDefaults function, because Colliders are not Behaviours, we need to disable/enable them separately.
        // Set the Coolider.
        Collider _col = GetComponent<Collider>();

        // We need to make sure that the collider is present.
        if (_col != null)
        {
            // If we have a Collider, disable it.
            _col.enabled = false;
        }

        // Show death effect at players position (we don't care about rotation).
        GameObject _gfxIns = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Destroy the effects after 3 seconds.
        Destroy(_gfxIns, 3f);


        // If we ARE the local player, we want to switch cameras.
        if (isLocalPlayer)
        {
            GameManager.singleton.SetSceneCameraActive(true);

            // Disable the UI.
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        // TODO: Remove debug.
        Debug.Log(transform.name + " is DEAD!.");

        // Call respawn method.
        StartCoroutine(Respawn());
    }

    // Respawn method.
    // An IEnumerator method is a void method.
    private IEnumerator Respawn ()
    {
        // Make the player wait for some  seconds before respawning.
        // We get the time from the MathSettings, stored in the GameManager singleton.
        // TODO: Set the GameManager.singleton.matchSettings as a variable for easy use.
        yield return new WaitForSeconds(GameManager.singleton.matchSettings.respawnTime);

        // Find a starting point using the NetworkManager.
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();

        // Translate player to position.
        transform.position = _spawnPoint.position;

        // Wait some seconds to make sure the player is moved before instantiating the particles.
        // TODO: Make sure this time plus the time used for respawning (↑) is equal to the matchSettings.respawnTime.
        yield return new WaitForSeconds(0.3f);
        
        // Set the player defaults.
        SetupPlayer();
        
    }

    void Update ()
    {
        // This is just for debugging, it should be commented.
        // This allow us to test dying and respawning without having to spawn another player.
        // Just press the K letter and the player dies.
        /*
        if (!isLocalPlayer)
        {
            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                RpcTakeDamage(999999999);
            }
        }
        */
    }

}
