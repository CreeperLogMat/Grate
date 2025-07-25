﻿using BepInEx.Configuration;
using Grate.Extensions;
using Grate.Gestures;
using Grate.GUI;
using UnityEngine;
using Player = GorillaLocomotion.GTPlayer;

namespace Grate.Modules.Movement;

public class Fly : GrateModule
{
    public static readonly string DisplayName = "Fly";

    public static ConfigEntry<int> Speed;
    public static ConfigEntry<int> Acceleration;
    private float speedScale = 10, acceleration = .01f;
    private Vector2 xz;
    private float y;

    private void FixedUpdate()
    {
        // nullify gravity by adding it's negative value to the player's velocity
        var rb = Player.Instance.bodyCollider.attachedRigidbody;
        if (enabledModules.ContainsKey(Bubble.DisplayName)
            && !enabledModules[Bubble.DisplayName])
            rb.AddForce(-UnityEngine.Physics.gravity * rb.mass * Player.Instance.scale);

        xz = GestureTracker.Instance.leftStickAxis.GetValue();
        y = GestureTracker.Instance.rightStickAxis.GetValue().y;

        var inputDirection = new Vector3(xz.x, y, xz.y);

        // Get the direction the player is facing but nullify the y axis component
        var playerForward = Player.Instance.bodyCollider.transform.forward;
        playerForward.y = 0;

        // Get the right vector of the player but nullify the y axis component
        var playerRight = Player.Instance.bodyCollider.transform.right;
        playerRight.y = 0;

        var velocity =
            inputDirection.x * playerRight +
            y * Vector3.up +
            inputDirection.z * playerForward;
        velocity *= Player.Instance.scale * speedScale;
        rb.velocity = Vector3.Lerp(rb.velocity, velocity, acceleration);
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;
        base.OnEnable();
        Plugin.menuController?.GetComponent<HandFly>().button.AddBlocker(ButtonController.Blocker.MOD_INCOMPAT);
        ReloadConfiguration();
    }

    public override string GetDisplayName()
    {
        return "Fly";
    }

    public override string Tutorial()
    {
        return "Use left stick to fly horizontally, and right stick to fly vertically.";
    }

    protected override void ReloadConfiguration()
    {
        speedScale = Speed.Value * 2;
        acceleration = Acceleration.Value;
        if (acceleration == 10)
            acceleration = 1;
        else
            acceleration = MathExtensions.Map(Acceleration.Value, 0, 10, 0.0075f, .25f);
    }

    public static void BindConfigEntries()
    {
        Speed = Plugin.configFile.Bind(
            DisplayName,
            "speed",
            5,
            "How fast you fly"
        );

        Acceleration = Plugin.configFile.Bind(
            DisplayName,
            "acceleration",
            5,
            "How fast you accelerate"
        );
    }

    protected override void Cleanup()
    {
        Plugin.menuController?.GetComponent<HandFly>().button.RemoveBlocker(ButtonController.Blocker.MOD_INCOMPAT);
    }
}