using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class SelectionMovementUI : MonoBehaviour
{
    [SerializeField] private Text[] movementTexts;

    private int currentSelectedMovement = 0;

    public void SetMovements(List<MoveBase> pokemonsMoves, MoveBase newMove)
    {
        currentSelectedMovement = 0;
        for (int i = 0; i < pokemonsMoves.Count; i++)
        {
            movementTexts[i].text = pokemonsMoves[i].Name;
        }
        movementTexts[pokemonsMoves.Count].text = newMove.Name;
    }

    public void HandleForgetMoveSelection(UnityAction<int> onSelected)
    {

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            int direction = Mathf.FloorToInt(Input.GetAxisRaw("Vertical"));
            currentSelectedMovement -= direction;

            onSelected?.Invoke(-1);
        }
        currentSelectedMovement = Mathf.Clamp(currentSelectedMovement, 0, PokemonBase.NUMBER_OF_LEARNABLE_MOVE);
        UpdateColorForgetMoveSelection(currentSelectedMovement);
        if (Input.GetAxisRaw("Submit") != 0)
        {
            onSelected?.Invoke(currentSelectedMovement);
        }
    }

    public void UpdateColorForgetMoveSelection(int selected)
    {
        for (int i = 0; i <= PokemonBase.NUMBER_OF_LEARNABLE_MOVE; i++)
        {
            movementTexts[i].color = (i == selected ? ColorManager.SharedInstance.selectedColor : Color.black);
        }
    }
}