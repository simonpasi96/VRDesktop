using UnityEngine;

public class GameObjectLink : MonoBehaviour
{
    [SerializeField]
    GameObject gOToCheck;
    [SerializeField]
    GameObject[] gameObjectsToChange;
    bool wasActive = true;


    private void Update()
    {
        if (wasActive != gOToCheck.activeInHierarchy)
        {
            if (gOToCheck.activeInHierarchy)
            {
                foreach (GameObject go in gameObjectsToChange)
                    go.SetActive(true);
                wasActive = true;
            }
            else
            {
                foreach (GameObject go in gameObjectsToChange)
                    go.SetActive(false);
                wasActive = false;
            }
        }
    }
}
