using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class PlayerDataHandlerForSingleplayer
{
    public GameObject parent;

    public Image bg;
    public TextMeshProUGUI name;
    public GameObject[] Avatars;

    public string color;
    public int player_id;

    public int TroopsInPossesion;

    public TextMeshProUGUI AiDifficulty;

    // Specifically For Ai Attack
    public string AiDifficultyValue;

    public List<int> CardNumbers = new List<int>();
}
