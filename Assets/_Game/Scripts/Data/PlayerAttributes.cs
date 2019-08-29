using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerAttributes", menuName = "Pandemic/Player Attributes")]
public class PlayerAttributes : ScriptableObject
{
    [SerializeField] private int m_maxHealth = 0;

    public int maxHealth { get { return m_maxHealth; } }
}
