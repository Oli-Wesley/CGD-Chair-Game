using UnityEngine;

namespace InteractionTypes
{
    // These are based of the rubbish types in Bristol. Happy for them to change/be refined.
    public enum RubbishType
    {
        NONE,
        CARDBOARD,
        PLASTIC,
        GLASS,
        METAL,
        FOOD,
        REFUSE
    }

    public enum InteractableState
    {
        IDLE,
        PICKING_UP,
        IN_INVENTORY,
        DROPPING_OFF,
        NONE // Shouldn't be used, here for now just in case
    }
    
    public static class RubbishHelper
    {
        public static Color GetColourForRubbishType(RubbishType rubbishType)
        {
            switch (rubbishType)
            {
                case RubbishType.CARDBOARD: return new Color32(35, 125, 193, 255);
                case RubbishType.PLASTIC: return new Color32(223, 35, 40, 255);
                case RubbishType.GLASS: return new Color32(19, 184, 165, 255);
                case RubbishType.METAL: return new Color32(85, 87, 92, 255);
                case RubbishType.FOOD: return new Color32(68, 202, 107, 255);
                case RubbishType.REFUSE: return new Color32(255, 255, 255, 255);
                default: return new Color32(255, 0, 255, 255);
            }
        }
    }
}


