using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] private LayerMask solidObjLayers, pokemonLayer, interactableLayer, playerLayer, fovLayer;

    public LayerMask SolidObjLayers => solidObjLayers;
    public LayerMask PokemonLayer => pokemonLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FovLayer => fovLayer;

    public static GameLayers SharedInstance;

    void Awake()
    {
        if (SharedInstance == null) SharedInstance = this;
    }

    public LayerMask CollisionLayers => SolidObjLayers | InteractableLayer | PlayerLayer;

}
