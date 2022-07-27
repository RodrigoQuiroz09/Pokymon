using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager SharedInstance;
    public Color selectedColor;

    private void Awake()
    {
        SharedInstance = this;
    }

    public Color BarColor(float finalScale)
    {
        if (finalScale < 0.2f) return new Color(193 / 255f, 45 / 255f, 45 / 255f);
        else if (finalScale < 0.5f) return new Color(211 / 255f, 212 / 255f, 29 / 255f);
        else return new Color(98 / 255f, 178 / 255f, 61 / 255f);

    }

    public Color PpColor(float finalScale)
    {
        if (finalScale < 0.2f) return new Color(193 / 255f, 45 / 255f, 45 / 255f);
        else if (finalScale < 0.5f) return new Color(211 / 255f, 212 / 255f, 29 / 255f);
        else return Color.black;

    }

    public class TypeColor
    {
        private static Color[] colors =
        {
            Color.white,//None
            new Color (0.8734059f,0.8773585f,0.8235582f), //Normal
            new Color (0.990566f,0.5957404f,0.5279903f), //Fire
            new Color (0.5613208f,0.7828107f,1), //Water
            new Color (0,0,0), //Electric
            new Color (0.4103774f,1,0.6946618f), //Grass
            new Color (0,0,0), //Ice
            new Color (0,0,0), //Fight
            new Color (0.6981132f,0.4774831f,0.6539872f), //Poisson
            new Color (0,0,0), //Ground
            new Color (0,0,0), //Fly
            new Color (0,0,0), //Phychic
            new Color (0.8193042f,0.9333333f,0.5254902f), //Bug
            new Color (0,0,0), //Rock
            new Color (0,0,0), //Ghost
            new Color (0.6556701f,0.5568628f,0.7647059f), //Dragon
            new Color (0.735849f,0.6178355f,0.5588287f), //Dark
            new Color (0,0,0), //Steel
            new Color (0,0,0), //Fairy
        };

        public static Color GetColorFromType(PokemonType type)
        {
            return colors[(int)type];
        }
    }

    public class StatusConditionColor
    {
        private static Dictionary<StatusConditionId, Color> colors = new Dictionary<StatusConditionId, Color>()
        {
            {StatusConditionId.NONE, Color.white},
            {StatusConditionId.BRN, new Color (223f/255, 134f/255, 67f/255)},
            {StatusConditionId.FRZ, new Color (168f/255, 214f/255, 215f/255)},
            {StatusConditionId.PAR, new Color (241f/255, 208f/255, 83f/255)},
            {StatusConditionId.PSN, new Color (147f/255, 73f/255, 156f/255)},
            {StatusConditionId.SLP, new Color (163f/255, 147f/255, 234f/255)}
        };
        public static Color GetColoFromStatusCondititon(StatusConditionId id)
        {
            return colors[id];
        }
    }


}
