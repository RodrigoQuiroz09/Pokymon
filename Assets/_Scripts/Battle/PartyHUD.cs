using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHUD : MonoBehaviour
{
    [SerializeField] private Text messageText;
    private PartyMemberHUD[] membersHUD;

    private List<Pokemon> pokemons;


    public void InitPartyHUD()
    {
        membersHUD = GetComponentsInChildren<PartyMemberHUD>(true);

    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        messageText.text = "Selecciona un Pokemon";
        for (int i = 0; i < membersHUD.Length; i++)
        {
            if (i < pokemons.Count)
            {
                membersHUD[i].gameObject.SetActive(true);
                membersHUD[i].SetPokemonData(pokemons[i]);
            }
            else
            {
                membersHUD[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateSelectedPokemon(int selectedPokemon)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            membersHUD[i].SetSelectedPokemon(i == selectedPokemon);
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

}
