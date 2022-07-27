using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    public Text pokemonName;
    public Text pokemonLvl;
    public HealthBar healthBar;

    public GameObject expBar;

    private Pokemon _pokemon;

    public GameObject StatusBox;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        pokemonName.text = pokemon.Base.Name;
        SetLevelText();
        healthBar.SetHP(_pokemon);
        SetXP();
        StartCoroutine(UpdatePokemonData());
        SetStatusConditionData();
        _pokemon.OnStatusConditionChange += SetStatusConditionData;
    }

    public IEnumerator UpdatePokemonData()
    {
        if (_pokemon.HasHPChange)
        {
            yield return StartCoroutine(healthBar.SetSmoothHP(_pokemon));
            _pokemon.HasHPChange = false;
        }
    }

    public void SetXP()
    {
        if (expBar == null) return;

        expBar.transform.localScale = new Vector3(NormalizedExp(), 1, 1);
    }

    public IEnumerator SetExpSmooth(bool needsToResetBar = false)
    {
        if (expBar == null) yield break;

        if (needsToResetBar) expBar.transform.localScale = new Vector3(0, 1, 1);

        yield return expBar.transform.DOScaleX(NormalizedExp(), 2f).WaitForCompletion();
    }

    float NormalizedExp()
    {

        float currentLevelExp = _pokemon.Base.GetNeccesaryExperienceForLevel(_pokemon.Level);
        float nextLevelExp = _pokemon.Base.GetNeccesaryExperienceForLevel(_pokemon.Level + 1);
        float normalizedExp = (_pokemon.Experience) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);

    }

    public void SetLevelText()
    {
        pokemonLvl.text = $"Lvl {_pokemon.Level}"; ;
    }

    void SetStatusConditionData()
    {
        if (_pokemon.StatusCondition == null) StatusBox.SetActive(false);
        else
        {
            StatusBox.SetActive(true);
            StatusBox.GetComponent<Image>().color = ColorManager.StatusConditionColor
                                                                .GetColoFromStatusCondititon(_pokemon.StatusCondition.Id);
            StatusBox.GetComponentInChildren<Text>().text = _pokemon.StatusCondition.Id.ToString();
        }
    }


}
