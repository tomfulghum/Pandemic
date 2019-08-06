using UnityEngine;

[CreateAssetMenu(fileName = "NewArea", menuName = "Pandemic/Area")]
public class Area : ScriptableObject
{
    [UniqueIdentifier]
    public string id;
    public string sceneName;
}
