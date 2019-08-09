using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFilesMenu : MonoBehaviour
{
    //************************//
    //    Public Functions    //
    //************************//

    public void LoadSaveFile(int _index)
    {
        GameManager.Instance.LoadSaveFile(_index);
    }

    public void CreateSaveFile(int _index)
    {
        GameManager.Instance.CreateSaveFile(_index);

    }

}
