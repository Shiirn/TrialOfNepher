using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemNature { Turn, Battle }

public class ItemCard {
    
    public int id;
    public string itemName;
    public string type;
    public ItemNature nature;
    public string description;
    public string function;
    public string spriteName;
    public Sprite cardImg;

    public ItemCard(int _id, string _name, string _type, string _nature, string _description, string sprite, string _function)
    {
        id = _id;
        itemName = _name;
        type = _type;

        switch (_nature)
        {
            case "Turn":
                nature = ItemNature.Turn;
                break;
            case "Battle":
                nature = ItemNature.Battle;
                break;
        }

        spriteName = "sprite" + sprite;

        description = _description;
        function = _function;        
    }
}
