using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberHUD : MonoBehaviour
{
    public Text nameText, lvlText, typeText, hpText;
    public HealthBar healthBar;
    public Image pokemonImage;

    private Pokemon _pokemon;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        lvlText.text = $"Lvl {pokemon.Level}";


        typeText.text = (pokemon.Base.Type2 != PokemonType.None
                        ? $"{pokemon.Base.Type1.ToString()} - {pokemon.Base.Type2.ToString()}"
                        : pokemon.Base.Type1.ToString());

        healthBar.SetHP(pokemon);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        GetComponent<Image>().color = ColorManager.TypeColor.GetColorFromType(pokemon.Base.Type1);
    }

    public void SetSelectedPokemon(bool selected)
    {
        if (selected)
        {
            nameText.color = ColorManager.SharedInstance.selectedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }

}
