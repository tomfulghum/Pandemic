using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private GameObject m_compassNeedle = default;
    [SerializeField] private GameObject m_compassNeedleInactive = default;
    [SerializeField] private GameObject m_keyChain = default;
    [SerializeField] private List<Sprite> m_keyChainSprites = default;

    //**********************//
    //    Private Fields    //
    //**********************//

    private List<NormalKey> m_keysInGame;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        UpdateKeyVisuals();
        //UpdateHealth();
        UpdateCompassNeedle();
    }

    private void OnEnable()
    {
        AreaTransitionManager.onAreaLoaded += UpdateKeylist;
    }
    private void OnDisable()
    {
        AreaTransitionManager.onAreaLoaded -= UpdateKeylist;
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void UpdateKeyVisuals()
    {
        GameObject player = GameManager.Instance.player;
        int numOfKeys = player.GetComponent<PlayerInventory>().normalKeyCount;
        if (numOfKeys <= 8) //8 == max num of keys
            m_keyChain.GetComponent<Image>().sprite = m_keyChainSprites[numOfKeys];
        else
            m_keyChain.GetComponent<Image>().sprite = m_keyChainSprites[8];
    }

    private void UpdateHealth()
    {

    }

    private void UpdateCompassNeedle()
    {
        List<GameObject> activeKeys = new List<GameObject>();
        foreach(NormalKey key in m_keysInGame)
        {
            if(key.gameObject.activeInHierarchy)
            {
                activeKeys.Add(key.gameObject);
            }
        }

        if(activeKeys.Count == 0)
        {
            m_compassNeedle.SetActive(false);
            m_compassNeedleInactive.SetActive(true);
        } else
        {
            if(m_compassNeedle.activeInHierarchy == false)
            {
                m_compassNeedle.SetActive(true);
                m_compassNeedleInactive.SetActive(false);
            }

            GameObject closestKey = FindClosestKey(activeKeys, GameManager.Instance.player);

            Vector2 facingDirection = closestKey.transform.position - GameManager.Instance.player.transform.position;
            if(facingDirection != Vector2.zero)
            {
                float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
                m_compassNeedle.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            }
        }
    }

    private GameObject FindClosestKey(List<GameObject> _keys, GameObject _player)
    {
        float smallestDistance = Mathf.Infinity;
        GameObject closestKey = null;

        foreach(GameObject key in _keys)
        {
            if(Vector2.Distance(key.transform.position, _player.transform.position) < smallestDistance)
            {
                closestKey = key;
                smallestDistance = Vector2.Distance(key.transform.position, _player.transform.position);
            }
        }

        return closestKey;
    }

    private void UpdateKeylist()
    {
        AreaController areaController = FindObjectOfType<AreaController>();
        if (areaController != null)
            m_keysInGame = areaController.keys;
    }
}
