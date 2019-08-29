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
        if (GameManager.Instance.GetSaveFileData(_index) == null) {
            GameManager.Instance.CreateSaveGame(_index);
        }

        GameManager.Instance.LoadSaveGame(_index);
    }
}
