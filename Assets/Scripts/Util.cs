using UnityEngine;


// This class will contain usefull methoods to use throughout the game.
public class Util {

    // This method will change the layer of the object we are passing.
    // It will also recursively change layers of the child objects.
    public static void SetLayerRecursively (GameObject _obj, int _newLayer)
    {
        if (_obj == null)
        {
            return;
        }
        else
        {
            // Change the object's layer.
            _obj.layer = _newLayer;

            // Iterate object's children.
            foreach (Transform _child in _obj.transform)
            {
                if (_child == null)
                {
                    continue;
                }
                else
                {
                    // Recursive God.
                    SetLayerRecursively(_child.gameObject, _newLayer);
                }
            }
        }
    }

}
