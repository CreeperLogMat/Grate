﻿using System;
using Grate.Extensions;
using Grate.Gestures;
using Grate.GUI;
using Grate.Networking;
using Grate.Patches;
using Grate.Tools;
using UnityEngine;
using UnityEngine.UI;
using NetworkPlayer = NetPlayer;

namespace Grate.Modules.Misc;

public class Bat : GrateModule
{
    public static readonly string DisplayName = "Bat";
    private static GameObject? bat;

    protected override void Start()
    {
        base.Start();
        if (bat == null)
        {
            bat = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("bat Variant"));
            bat.transform.SetParent(GestureTracker.Instance.rightHand.transform, true);
            bat.transform.localPosition = new Vector3(-0.4782f, 0.1f, 0.4f);
            bat.transform.localRotation = Quaternion.Euler(9, 0, 0);
            bat.transform.localScale /= 2;
            bat.SetActive(false);
        }

        NetworkPropertyHandler.Instance.OnPlayerModStatusChanged += OnPlayerModStatusChanged;
        VRRigCachePatches.OnRigCached += OnRigCached;
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;
        base.OnEnable();
        try
        {
            GestureTracker.Instance.rightGrip.OnPressed += ToggleBatOn;
            GestureTracker.Instance.rightGrip.OnReleased += ToggleBatOff;
        }
        catch (Exception e)
        {
            Logging.Exception(e);
        }
    }

    private void OnPlayerModStatusChanged(NetworkPlayer player, string mod, bool modEnabled)
    {
        if (mod == DisplayName && player != NetworkSystem.Instance.LocalPlayer && player.IsSupporter())
        {
            if (modEnabled)
                player.Rig()?.gameObject.GetOrAddComponent<Bat>();
            else
                Destroy(player.Rig()?.gameObject.GetComponent<Bat>());
        }
    }


    private void ToggleBatOn(InputTracker tracker)
    {
        bat?.SetActive(true);
    }

    private void ToggleBatOff(InputTracker tracker)
    {
        bat?.SetActive(false);
    }

    protected override void Cleanup()
    {
        bat?.SetActive(false);
        if (GestureTracker.Instance != null)
        {
            GestureTracker.Instance.rightGrip.OnPressed -= ToggleBatOn;
            GestureTracker.Instance.rightGrip.OnReleased -= ToggleBatOff;
        }
    }

    private void OnRigCached(NetPlayer player, VRRig rig)
    {
        rig?.gameObject?.GetComponent<Bat>()?.Obliterate();
    }

    public override string GetDisplayName()
    {
        return DisplayName;
    }

    public override string Tutorial()
    {
        return "Bonk.";
    }

    private class Netbonk : MonoBehaviour
    {
        private NetworkedPlayer networkedPlayer;
        private GameObject bat;

        private void OnEnable()
        {
            networkedPlayer = gameObject.GetComponent<NetworkedPlayer>();
            var rightHand = networkedPlayer.rig.rightHandTransform;

            bat = Instantiate<GameObject>(Bat.bat);

            bat.transform.SetParent(rightHand);
            bat.transform.localPosition = new Vector3(0.04f, 0.05f, -0.02f);
            bat.transform.localRotation = Quaternion.Euler(78.4409f, 0, 0);
            bat.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            networkedPlayer.OnGripPressed += OnGripPressed;
            networkedPlayer.OnGripReleased += OnGripReleased;
        }

        void OnTriggerEnter(GameObject self, GameObject other)
        {
            if (other.layer == LayerMask.NameToLayer("Gorilla Tag Collider"))
            {
                Bat.bat.GetComponent<AudioSource>().Play();
            }
        }

        private void OnDisable()
        {
            bat.Obliterate();
            networkedPlayer.OnGripPressed -= OnGripPressed;
            networkedPlayer.OnGripReleased -= OnGripReleased;
        }

        private void OnDestroy()
        {
            bat.Obliterate();
            networkedPlayer.OnGripPressed -= OnGripPressed;
            networkedPlayer.OnGripReleased -= OnGripReleased;
        }

        private void OnGripPressed(NetworkedPlayer player, bool isLeft)
        {
            if (!isLeft) bat.SetActive(true);
        }

        private void OnGripReleased(NetworkedPlayer player, bool isLeft)
        {
            if (!isLeft) bat.SetActive(false);
        }
    }
}