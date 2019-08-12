using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveFileButton : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private GameObject m_newGameText = default;
    [SerializeField] private GameObject m_saveFileData = default;
    [SerializeField] private TextMeshProUGUI m_healthText = default;
    [SerializeField] private TextMeshProUGUI m_currencyText = default;
    [SerializeField] private TextMeshProUGUI m_playtimeText = default;
    [SerializeField] private TextMeshProUGUI m_areaText = default;
    [SerializeField] private int m_index = 0;

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
