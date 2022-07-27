using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerController : MonoBehaviour
{
    private Vector2 input;
    private Character _character;
    private float timeSinceLastClick;
    [SerializeField] string playerName;
    [SerializeField] Sprite playerSprite;
    [SerializeField] float timeBetweenClicks = 0.5f;
    public UnityAction OnPokemonEncountered;
    public UnityAction<Collider2D> OnEnterTrainerFov;

    public string PlayerName => playerName;
    public Sprite PlayerSprite => playerSprite;
    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (!_character.isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input != Vector2.zero)
            {
                StartCoroutine(_character.MoveTowards(input, () =>
                {
                    OnMoveFinish();
                }

                ));

            }
        }
        if (Input.GetAxisRaw("Submit") != 0)
        {
            if (timeSinceLastClick >= timeBetweenClicks) Interact();
        }
        _character.HandleUpdate();
    }

    void OnMoveFinish()
    {
        CheckForPokemon();
        CheckForInTrainerFoV();
    }

    /// <summary>
    /// Método que comprueba que la zona a la que queremos acceder, esté disponible
    /// </summary>
    /// <param name="target">Zona a la que queremos acceder</param>
    /// <returns>Devuelve true, si el target está disponible y false en caso contrario</returns>
    private void Interact()
    {
        timeSinceLastClick = 0;
        var facingDirection = new Vector3(_character.Animator.MoveX, _character.Animator.MoveY);
        var interactPosition = transform.position + facingDirection;
        Debug.DrawLine(transform.position, interactPosition, Color.magenta, 1f);
        var collider = Physics2D.OverlapCircle(interactPosition, 0.2f, GameLayers.SharedInstance.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform.position);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] float verticalOffset = 0.2f;
    private void CheckForPokemon()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset, 0), 0.2f, GameLayers.SharedInstance.PokemonLayer) != null)
        {
            if (Random.Range(0, 100) < 40)
            {
                _character.Animator.isMoving = false;
                OnPokemonEncountered();
            }
        }
    }

    private void CheckForInTrainerFoV()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset, 0), 0.2f, GameLayers.SharedInstance.FovLayer);
        if (collider != null)
        {
            OnEnterTrainerFov?.Invoke(collider);
            _character.Animator.isMoving = false;

            Debug.Log("en el Fov del trainer");
        }
    }
}
