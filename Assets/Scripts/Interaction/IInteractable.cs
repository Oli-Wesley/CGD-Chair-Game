using UnityEngine;
using InteractionTypes;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public class IInteractable : MonoBehaviour
    {
        public RubbishType rubbishType;
        public InteractableState interactableState = InteractableState.IDLE;
    
        // This is done the way it is below for alpha. Once we have a model, the code below can essentially be removed
        private void Awake() => UpdateRubbishColour();
        private void OnValidate() => UpdateRubbishColour();

        public void UpdateRubbishColour()
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Color colour = RubbishHelper.GetColourForRubbishType(rubbishType);
            renderer.sharedMaterial.SetColor("_Color", colour);
        }
    } 
}


