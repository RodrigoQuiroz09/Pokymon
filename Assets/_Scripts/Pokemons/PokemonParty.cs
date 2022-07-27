using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] private List<Pokemon> pokemons;

    public const int NUM_MAX_POKEMON_IN_PARTY = 6;

    public List<Pokemon> Pokemons { get => pokemons; }

    private void Start()
    {
        foreach (var item in pokemons)
        {
            item.InitPokemon();
        }
    }

    public Pokemon GetFirstHealthyPokemon()
    {
        return pokemons.Where(p => p.HP > 0).FirstOrDefault();
    }

    public int GetPositionFromPokemon(Pokemon playerPokemon)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (playerPokemon == pokemons[i])
            {
                return i;
            }
        }
        return -1;
    }

    public bool AddPokemonToParty(Pokemon pokemon)
    {
        if (pokemons.Count < NUM_MAX_POKEMON_IN_PARTY)
        {
            pokemons.Add(pokemon);
            return true;
        }
        else
        {
            return false;
        }
    }
}
