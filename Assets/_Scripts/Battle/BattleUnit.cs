using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleUnit : MonoBehaviour
{
    public PokemonBase _base;
    public int _level;

    [SerializeField] bool isPlayer;
    public bool IsPlayer => isPlayer;

    public Pokemon Pokemon { get; set; }
    private Image pokemonImage;

    Vector3 initialPos;
    [SerializeField]
    private float startTimeAnim = 1.0f, attackTimeAnim = 0.3f,
                                    dieTimeAnim = 1f, hitTimeAnim = 0.1f, caputerdTimeAnim = 0.5f;
    private Color intialColor;

    [SerializeField] BattleHUD hud;

    public BattleHUD HUD => hud;

    private void Awake()
    {
        pokemonImage = GetComponent<Image>();
        initialPos = pokemonImage.transform.localPosition;
        intialColor = pokemonImage.color;
    }


    public void SetupPokemon(Pokemon pokemon)
    {
        Pokemon = pokemon;
        pokemonImage.sprite = (isPlayer ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite);
        pokemonImage.color = intialColor;
        hud.gameObject.SetActive(true);
        hud.SetPokemonData(Pokemon);
        transform.localScale = new Vector3(1, 1, 1);
        PlayStartAnimation();
    }

    public void PlayStartAnimation()
    {
        pokemonImage.transform.localPosition = new Vector3(initialPos.x + (isPlayer ? -1 : 1) * 400, initialPos.y);
        pokemonImage.transform.DOLocalMoveX(initialPos.x, startTimeAnim);
    }

    public void PlayAttackAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.transform.DOLocalMoveX(initialPos.x + (isPlayer ? -1 : 1) * 50, attackTimeAnim));
        seq.Append(pokemonImage.transform.DOLocalMoveX(initialPos.x, attackTimeAnim));

    }

    public void PlayReceiveAttackAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOColor(Color.gray, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(intialColor, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(Color.gray, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(intialColor, hitTimeAnim));
    }

    public void PlayFaintAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.transform.DOLocalMoveY(initialPos.y - 400, dieTimeAnim));
        seq.Join(pokemonImage.DOFade(0f, dieTimeAnim));

    }

    public IEnumerator PlayCaptureAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(0, caputerdTimeAnim));
        seq.Join(this.transform.DOScale(new Vector3(0.25f, 0.25f, 1f), caputerdTimeAnim));
        seq.Join(transform.DOLocalMoveY(initialPos.y + 50f, caputerdTimeAnim));
        yield return seq.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(1, caputerdTimeAnim));
        seq.Join(this.transform.DOScale(new Vector3(1f, 1f, 1f), caputerdTimeAnim));
        seq.Join(transform.DOLocalMoveY(initialPos.y, caputerdTimeAnim));
        yield return seq.WaitForCompletion();
    }

    public void ClearHUD()
    {
        hud.gameObject.SetActive(false);
    }

}
