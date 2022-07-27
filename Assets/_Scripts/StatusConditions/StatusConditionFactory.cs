using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusConditionFactory
{
    public static void InitFactory()
    {
        foreach (var condition in StatusConditions)
        {
            var id = condition.Key;
            var statusCond = condition.Value;
            statusCond.Id = id;
        }
    }
    public static Dictionary<StatusConditionId, StatusCondition> StatusConditions { get; set; } =

        new Dictionary<StatusConditionId, StatusCondition>()
        {
            {
                StatusConditionId.PSN,
                new StatusCondition()
                {
                    Name ="Poison",
                    StartMessage= "ha sido envenenado",
                    Description = "Hace que el Pokemon sufra daño en cada turno",
                    OnFinishTurn = PoisionEffect
                }
            },
            {
                StatusConditionId.BRN,
                new StatusCondition()
                {
                    Name ="Burned",
                    StartMessage= "ha sido quemado",
                    Description = "Hace que el Pokemon sufra daño en cada turno",
                    OnFinishTurn = BurnedEffect
                }
            },
            {
                StatusConditionId.PAR,
                new StatusCondition()
                {

                    Name ="Paralyzed",
                    StartMessage= "ha sido paralizado",
                    Description = "Hace que el Pokemon pueda estar paralizado en el turno",
                    OnStartTurn  = ParalyzedEffect
                }
            },
            {
                StatusConditionId.FRZ,
                new StatusCondition()
                {
                    Name ="Paralyzed",
                    StartMessage= "ha sido congelado",
                    Description = "Hace que el Pokemon pesté congelado, pero se puede curar aleatoriamente en un turno",
                    OnStartTurn  = FrozenEffect
                }
            },
            {
                StatusConditionId.SLP,
                new StatusCondition()
                {
                    Name ="Sleep",
                    StartMessage= "se ha dormido",
                    Description = "Hace que el Pokemon duerma durante un numero fijo de turno",
                    OnApplyStatusCondition = (Pokemon pokemon) =>
                    {
                       pokemon.StatusNumberTurns = Random.Range(1,4);
                       Debug.Log($"El pokemon dormirá durante {pokemon.StatusNumberTurns} turnos");
                    },
                    OnStartTurn  = (Pokemon pokemon) =>
                    {
                        if(pokemon.StatusNumberTurns <=0)
                        {
                            pokemon.CureStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} se ha despertado");
                            return true;
                        }
                        pokemon.StatusNumberTurns--;
                        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue dormido");
                        return false;
                    }
                }
            } ,
            {
                StatusConditionId.CONF,
                new StatusCondition()
                {
                    Name ="Confusion",
                    StartMessage= "se ha confundido",
                    Description = "Hace que el Pokemon esté confundido y pueda atacarse a sí mismo",
                    OnApplyStatusCondition = (Pokemon pokemon) =>
                    {
                       pokemon.VolatileStatusNumberTurns = Random.Range(1,6);
                       Debug.Log($"El pokemon estara confundido durante {pokemon.VolatileStatusNumberTurns} turnos");
                    },
                    OnStartTurn  = (Pokemon pokemon) =>
                    {
                        if(pokemon.VolatileStatusNumberTurns <=0)
                        {
                            pokemon.CureVolatileStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} se ha recuperado del estado Confusión");
                            return true;
                        }
                        pokemon.VolatileStatusNumberTurns--;
                        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue confundido");
                        if(Random.Range(0,2)==0)
                        {
                            return true;
                        }
                        pokemon.UpdateHP(pokemon.MaxHP/6);
                        pokemon.StatusChangeMessages.Enqueue("Tan confuso que se hiere a sí mismo");

                        return false;
                    }
                }
            }
        };

    static void PoisionEffect(Pokemon pokemon)
    {
        pokemon.UpdateHP(Mathf.CeilToInt(pokemon.MaxHP / 8f));
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos del veneno");
    }

    static void BurnedEffect(Pokemon pokemon)
    {
        pokemon.UpdateHP(Mathf.CeilToInt(pokemon.MaxHP / 15f));
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos de la quemadura");
    }

    static bool ParalyzedEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 35)
        {
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está paralizado y no puede moverse");
            return false;
        }
        return true;
    }

    static bool FrozenEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 25)
        {
            pokemon.CureStatusCondition();
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está congelado");
            return true;
        }

        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue congelado");
        return false;
    }

}

public enum StatusConditionId
{
    NONE, BRN, FRZ, PAR, PSN, SLP, CONF
}