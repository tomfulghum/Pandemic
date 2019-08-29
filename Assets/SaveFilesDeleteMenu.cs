using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFilesDeleteMenu : MonoBehaviour
{
    private int m_index;

    public void SetIndexToDelete(int _index)
    {
        m_index = _index;
    }

    public void DeleteSaveFile()
    {
        GameManager.Instance.DeleteSaveGame(m_index);

    }
}
