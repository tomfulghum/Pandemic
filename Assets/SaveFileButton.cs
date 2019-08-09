using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveFileButton : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private GameObject m_newGameText;
    [SerializeField] private GameObject m_saveFileData;
    [SerializeField] private TextMeshProUGUI m_healthText;
    [SerializeField] private TextMeshProUGUI m_currencyText;
    [SerializeField] private TextMeshProUGUI m_playtimeText;
    [SerializeField] private TextMeshProUGUI m_areaText;
    [SerializeField] private int m_index;

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        SaveFileData data = GameManager.Instance.GetSaveFileData(m_index);
        if (data != null) {
            m_newGameText.SetActive(false);
            m_saveFileData.SetActive(true);
            m_healthText.text = "" + data.health;
            m_currencyText.text = "" + data.currency;
            m_playtimeText.text = "" + data.playTime;
            m_areaText.text = data.area;
        } else {
            m_newGameText.SetActive(true);
            m_saveFileData.SetActive(false);
        }
    }
}
