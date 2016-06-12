// In this file we are going to ignore components that are not from the player.
// Without this script, when a player moves, the other player moves.
// This happens because they are both using the same components.
// We need to disable other peoples components.

using UnityEngine;

// This allows us to use the Networking High Level API.
using UnityEngine.Networking;

// We need a Player component to add it to the GameManager dictionary.
[RequireComponent(typeof(Player))]

// We need a PlayerController for fuel management.
[RequireComponent(typeof(PlayerController))]

// To use networking behaviour, we need to inherit from NetworkBehaviour.
public class PlayerSetup : NetworkBehaviour {

    // This array will be the list of compenents we want to disable.
    [SerializeField]
    Behaviour[] componentsToDisable;

    // We have to store the name for the RemotePlayer layer.
    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    // Store the DontDraw layer. This layer will be hidden for the current player.
    [SerializeField]
    string dontDrawLayerName = "DontDraw";

    // Reference to the playerGraphics so we can add the DontDraw layer.
    [SerializeField]
    GameObject playerGraphics;

    // Reference the player UI where the Crosshair is.
    // We want to instatiate this when the player joins and remove it when the player leaves.
    [SerializeField]
    GameObject playerUIPrefab;
    // We'll make this public so we can disable it on death.
    [HideInInspector]
    public GameObject playerUIInstance;

    void Start ()
    {
        // First we need to check if the user is not the Local Player.
        if (!isLocalPlayer)
        {
            // Then we should disable components.
            DisableComponents();

            // Assign the RemotePlayer layer.
            AssignRemoteLayer();

        }
        else
        {
            // Disable player graphics for local player.
            Util.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            // Create the playerUI and store it.
            playerUIInstance = Instantiate(playerUIPrefab);
            // Keep the same name (if not added, Unity adds a 'clone' string at the end).
            playerUIInstance.name = playerUIPrefab.name;

            // Configure player UI.
            // Get the player UI.
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();

            // Check that the UI is actually there.
            if (ui == null)
            {
                // Throw error.
                Debug.LogError("PlayerSetup: No PlayerUI component on PlayerUI prefab.");
            }
            else
            {
                // Set the PlayerController for the UI.
                ui.SetController(GetComponent<PlayerController>());
            }

            // Now we need to setup the player (Player script).
            // Sets health, death, components, etc.
            GetComponent<Player>().SetupPlayer();
        }
        
    }

    // This is a NetworkBehaviour method.
    // This is called every time a client is setup locally.
    public override void OnStartClient()
    {
        // Leave default behaviour.
        base.OnStartClient();

        // Now lets add some behaviour of our own.
        // We will register the player with our GameManager.
        // First get the player's netID usign NetworkIdentity. Cast to string.
        string _netID = GetComponent<NetworkIdentity>().netId.ToString();

        // Now lets get the player itself.
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    // This method disables all the components from the componentsToDisable array.
    void DisableComponents() {
        // Loop the array.
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            // Disable all the components.──▄██▄
            // ─────────────────────────────▀███
            // ───────────────▄▄▄▄▄────────────█
            // ──────────────▀▄────▀▄──────────█
            // ──────────▄▀▀▀▄─█▄▄▄▄█▄▄─▄▀▀▀▄──█
            // ─────────█──▄──█────────█───▄─█─█
            // ─────────▀▄───▄▀────────▀▄───▄▀─█
            // ──────────█▀▀▀────────────▀▀▀─█─█
            // ▄▀▄▄▀▄────█──▄█▀█▀█▀█▀█▀█▄────█─█
            // █▒▒▒▒█────█──█████████████▄───█─█
            // █▒▒▒▒█────█──██████████████▄──█─█
            // █▒▒▒▒█────█───██████████████▄─█─█
            // █▒▒▒▒█────█────██████████████─█─█
            // █▒▒▒▒█────█───██████████████▀─█─█
            // █▒▒▒▒█───██───██████████████──█─█
            // ▀████▀──██▀█──█████████████▀──█▄█
            // ──██───██──▀█──█▄█▄█▄█▄█▄█▀──▄█▀
            // ──██──██────▀█─────────────▄▀▓█
            // ──██─██──────▀█▀▄▄▄▄▄▄▄▄▄▀▀▓▓▓█
            // ──████────────█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
            // ──███─────────█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
            componentsToDisable[i].enabled = false;
        }
    }

    // This method assigns the RemotePlayer layer to the player. ♬ ♬ ♬
    void AssignRemoteLayer()
    {
        // Assign the layer.
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    // This is a Unity method. It is call when an object is destroyed.
    void OnDisable ()
    {
        // Destroy the player UI we once created.
        Destroy (playerUIInstance);

        // If the player is destroyed, lets enable the camera.
        //First check that the player is the local player.
        if (isLocalPlayer)
        {
            // Now switch cameras.
            GameManager.singleton.SetSceneCameraActive(true);
        }

        // We need to remove the player from the GameManager dictionary.
        // We pass it the name so the GameManager can look for it.
        GameManager.UnregisterPlayer(transform.name);
    }

}
