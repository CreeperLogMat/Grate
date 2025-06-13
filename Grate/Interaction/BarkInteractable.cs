﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grate.Gestures;
using UnityEngine;

namespace Grate.Interaction;

public class GrateInteractable : MonoBehaviour
{
    public GrateInteractor[] validSelectors;
    public List<GrateInteractor> selectors = new();
    public List<GrateInteractor> hoverers = new();
    public int priority;
    public bool Activated;
    public bool Primary;
    private GestureTracker gt;
    public Action<GrateInteractable, GrateInteractor> OnActivateEnter, OnActivateExit;
    public Action<GrateInteractable, GrateInteractor> OnHoverEnter, OnHoverExit;
    public Action<GrateInteractable, GrateInteractor> OnPrimaryEnter, OnPrimaryExit;
    public Action<GrateInteractable, GrateInteractor> OnSelectEnter, OnSelectExit;
    public bool Selected => selectors.Count > 0;

    protected virtual void Awake()
    {
        gt = GestureTracker.Instance;
        gameObject.layer = GrateInteractor.InteractionLayer;
        validSelectors = new[] { gt.leftPalmInteractor, gt.rightPalmInteractor };
    }

    protected virtual void OnDestroy()
    {
        foreach (var hoverer in hoverers)
            hoverer?.hovered.Remove(this);
        foreach (var selector in selectors)
            selector?.selected.Remove(this);
    }

    protected virtual void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<GrateInteractor>() is GrateInteractor interactor)
        {
            if (!CanBeSelected(interactor) || interactor.hovered.Contains(this)) return;
            if (interactor.Selecting)
            {
                interactor.Select(this);
            }
            else
            {
                interactor.Hover(this);
                hoverers.Add(interactor);
                OnHoverEnter?.Invoke(this, interactor);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider collider)
    {
        if (!enabled) return;
        if (collider.GetComponent<GrateInteractor>() is GrateInteractor interactor)
        {
            if (!interactor.hovered.Contains(this)) return;
            interactor.hovered.Remove(this);
            hoverers.Remove(interactor);
            OnHoverExit?.Invoke(this, interactor);
        }
    }

    public virtual bool CanBeSelected(GrateInteractor interactor)
    {
        return enabled && !Selected && validSelectors.Contains(interactor);
    }

    public virtual void OnSelect(GrateInteractor interactor)
    {
        selectors.Add(interactor);
        OnSelectEnter?.Invoke(this, interactor);
    }

    public virtual void OnDeselect(GrateInteractor interactor)
    {
        selectors.Remove(interactor);
        OnSelectExit?.Invoke(this, interactor);
    }

    public virtual void OnActivate(GrateInteractor interactor)
    {
        Activated = true;
        OnActivateEnter?.Invoke(this, interactor);
    }

    public virtual void OnDeactivate(GrateInteractor interactor)
    {
        Activated = false;
        OnActivateExit?.Invoke(this, interactor);
    }

    public virtual void OnPrimary(GrateInteractor interactor)
    {
        Primary = true;
        OnPrimaryEnter?.Invoke(this, interactor);
    }

    public virtual void OnPrimaryReleased(GrateInteractor interactor)
    {
        Primary = false;
        OnPrimaryExit?.Invoke(this, interactor);
    }
}