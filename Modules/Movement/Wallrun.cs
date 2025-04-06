﻿using GorillaLocomotion;
using Grate.Tools;
using UnityEngine;
using System.Reflection;
using Grate.Modules.Physics;
using Grate.GUI;
using BepInEx.Configuration;

namespace Grate.Modules.Movement
{
    public class Wallrun : GrateModule
    {
        public static readonly string DisplayName = "Wall Run";
        private Vector3 baseGravity;
        private RaycastHit hit;
        void Awake()
        {
            baseGravity = UnityEngine.Physics.gravity;
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
        }

        protected void FixedUpdate()
        {
            GTPlayer player = GTPlayer.Instance;
            if (player.wasLeftHandColliding || player.wasRightHandColliding)
            {
                FieldInfo fieldInfo = typeof(GTPlayer).GetField("lastHitInfoHand", BindingFlags.NonPublic | BindingFlags.Instance);
                hit = (RaycastHit)fieldInfo.GetValue(player);
                UnityEngine.Physics.gravity = hit.normal * -baseGravity.magnitude * GravScale();
            }
            else
            {
                if (Vector3.Distance(player.bodyCollider.transform.position, hit.point) > 2 * GTPlayer.Instance.scale)
                    Cleanup();
            }
        }
        public float GravScale()
        {
            return LowGravity.Instance.active ? LowGravity.Instance.gravityScale : Power.Value;
        }

        public static ConfigEntry<int> Power;
        public static void BindConfigEntries()
        {
            Power = Plugin.configFile.Bind(
                section: DisplayName,
                key: "power",
                defaultValue: 1,
                description: "Wall Run Strength"
            );
        }

        protected override void Cleanup()
        {
            UnityEngine.Physics.gravity = baseGravity;
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Allows you to walk on any surface, no matter the angle.";
        }

    }
}


