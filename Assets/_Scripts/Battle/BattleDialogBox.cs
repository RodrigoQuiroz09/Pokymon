using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelect;
    [SerializeField] GameObject movementSelect;
    [SerializeField] GameObject movementDescriptionSelect;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> movementTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] GameObject yesNoBox;
    [SerializeField] Text yesText;
    [SerializeField] Text noText;
    public bool isWriting = false;

    public float characterPerSecond = 10.0f;



    public IEnumerator SetDialog(string message)
    {
        isWriting = true;
        dialogText.text = "";
        foreach (var character in message)
        {
            if (character != ' ') SoundManager.SharedInstance.PlayRandomCharacterSound();
            dialogText.text += character;
            yield return new WaitForSeconds(1 / characterPerSecond);
        }
        yield return new WaitForSeconds(1.0f);
        isWriting = false;
    }

    public void ToggleDialogText(bool activated)
    {
        dialogText.enabled = activated;
    }

    public void ToggleActions(bool activated)
    {
        actionSelect.SetActive(activated);
    }

    public void ToggleMovements(bool activated)
    {
        movementSelect.SetActive(activated);
        movementDescriptionSelect.SetActive(activated);
    }
    public void ToggleYesNoBox(bool activated)
    {
        yesNoBox.SetActive(activated);
    }
    public void SelectAction(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            actionTexts[i].color = (i == selectedAction ? ColorManager.SharedInstance.selectedColor : Color.black);

        }
    }

    public void SetPokemonMovements(List<Move> moves)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            movementTexts[i].text = (i < moves.Count ? moves[i].Base.Name : "---");
        }
    }

    public void SelectYesNoAction(bool yesSelected)
    {

        if (yesSelected)
        {
            yesText.color = ColorManager.SharedInstance.selectedColor;
            noText.color = Color.black;
        }
        else
        {
            noText.color = ColorManager.SharedInstance.selectedColor;
            yesText.color = Color.black;
        }

    }

    public void SelectMovement(int selectedMovement, Move move)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            movementTexts[i].color = (i == selectedMovement ? ColorManager.SharedInstance.selectedColor : Color.black);
        }
        ppText.text = $"PP {move.Pp}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString().ToUpper();

        ppText.color = ColorManager.SharedInstance.PpColor((float)move.Pp / move.Base.PP);
        movementDescriptionSelect.GetComponent<Image>().color = ColorManager.TypeColor.GetColorFromType(move.Base.Type);

    }



}
