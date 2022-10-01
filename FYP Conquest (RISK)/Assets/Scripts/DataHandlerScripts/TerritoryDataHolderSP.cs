using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TerritoryDataHolderSP
{
    public int Territory_id;

    public string Territory_name;

    public Image Territory_image;

    public Text totalTroopsText;

    public Button Territory_button;

    public GameObject AttackTerritoryImage;

    public GameObject[] Neigbhors;

    public int CapturedBy;
}
