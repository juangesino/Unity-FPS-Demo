// This is just going to be a class to store important match variables.

// System.Serializable allows me to change class attributes.
[System.Serializable]
public class MatchSettings {

    // Set a default respawnTime.
    // This is the time the player waits after dying to start playing again.
    public float respawnTime = 3f;
	
}
