using UnityEngine;
using System.Collections.Generic;

// In this class we will store user IDs to identify players.
public class GameManager : MonoBehaviour {

    // Create a singleton for the GameManager (see awake method).
    public static GameManager singleton;

    // Define a MatchSettings class to use important variables.
    public MatchSettings matchSettings;

    // Player ID prefix.
    private const string PLAYER_ID_PREFIX = "Player ";

    // We will create a dictionary that takes a playerID and gives us a Player.
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    // We'll create a sceneCamera variable to disable it while playing and enable it when leaving the game.
    // The scene camera is the main camera, it's the camera shown when waiting to join a game.
    [SerializeField]
    private GameObject sceneCamera;

    // Implement singleton.
    void Awake ()
    {
        Debug.Log("GamaManager: Awake method called.");

        // Check if the singleton is already initialized.
        if (singleton != null)
        {
            // It there's already a singleton we want to rise and error.
            Debug.LogError ("GameManager: More than one GameManager in the scene.");
        }
        else
        {
            // Assign the singleton.
            singleton = this;
        }
    }

    // Enable and disable the sceneCamera.
    public void SetSceneCameraActive (bool isActive)
    {
        // Check if the sceneCamera is there.
        if (sceneCamera == null)
        {
            return;
        }

        // Set camera to given state.
        sceneCamera.SetActive(isActive);
    }

    // This method is responsible for adding players to the dictionary.
    // It takes as argument the NetworkIdentifier ID and the player component.
    public static void RegisterPlayer(string _netID, Player _player)
    {
        // We now use the prefix to build a player ID.
        string _playerID = PLAYER_ID_PREFIX + _netID;

        // And now we add it to the dictionary.
        // We first pass the key and then the value.
        players.Add(_playerID, _player);

        // Now lets rename the player transfrom (shown in hierarchy).
        _player.transform.name = _playerID;
    }

    // Method to remove players form the dictionary.
    public static void UnregisterPlayer(string _playerID)
    {
        // Remove from the dictionary.
        players.Remove(_playerID);
    }

    // Method that gets a player matching an ID.
    public static Player GetPlayer (string _playerID)
    {
        // Just return the player that matches that ID in the dictionary.
        return players[_playerID];
    }

    /*
    // This builds a GUI to visualize players dictionary.
    void OnGUI ()
    {
        // Start a GUI rectangle.
        GUILayout.BeginArea(new Rect(200, 200, 200, 500));

        // Start a Vertical GUI Layout.
        GUILayout.BeginVertical();

        // Iterate dictionary keys.
        foreach (string _playerID in players.Keys)
        {
            // Create a GUI Label containing the playerID and the player name.
            GUILayout.Label(_playerID + "    -    " + players[_playerID].transform.name);
        }

        // End the Vertical GUI Layout.
        GUILayout.EndVertical();

        // End the GUI rectangle.
        GUILayout.EndArea();
    }
    */

}
