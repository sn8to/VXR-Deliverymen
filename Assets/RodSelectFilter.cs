using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class RodSelectFilter : MonoBehaviour, IXRSelectFilter
{
    public XRGrabInteractable rod;
    public bool requireRodHeld;

    public bool canProcess => isActiveAndEnabled;

    void OnEnable() => GetComponent<XRBaseInteractable>().selectFilters.Add(this);
    void OnDisable() => GetComponent<XRBaseInteractable>().selectFilters.Remove(this);

    bool HeldBy(IXRSelectInteractor interactor)
    {
        foreach (var held in rod.interactorsSelecting)
            if (held == interactor) return true;
            return false;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (requireRodHeld) return rod.isSelected && !HeldBy(interactor);
        return !rod.isSelected || HeldBy(interactor);
    }
}
