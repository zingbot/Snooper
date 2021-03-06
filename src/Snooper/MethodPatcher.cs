﻿// <copyright file="MethodPatcher.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace Snooper
{
    using System;
    using System.Reflection;
    using Harmony;

    /// <summary>
    /// A class that uses Harmony library for redirecting the game's methods.
    /// </summary>
    internal sealed class MethodPatcher
    {
        private const string HarmonyId = "com.cities_skylines.dymanoid.snooper";

        private readonly Patcher patcher;
        private readonly IPatch[] patches;

        /// <summary>Initializes a new instance of the <see cref="MethodPatcher"/> class.</summary>
        /// <param name="patches">The patches to process by this object.</param>
        /// <exception cref="ArgumentException">Thrown when no patches specified.</exception>
        public MethodPatcher(params IPatch[] patches)
        {
            if (patches == null || patches.Length == 0)
            {
                throw new ArgumentException("At least one patch is required");
            }

            this.patches = patches;
            var harmony = HarmonyInstance.Create(HarmonyId);
            patcher = new Patcher(harmony);
        }

        /// <summary>Applies all patches this object knows about.</summary>
        public void Apply()
        {
            try
            {
                Revert();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("The 'Snooper' mod failed to clean up methods before patching: " + ex);
            }

            foreach (IPatch patch in patches)
            {
                patch.ApplyPatch(patcher);
            }
        }

        /// <summary>Reverts all patches, if any applied.</summary>
        public void Revert()
        {
            foreach (IPatch patch in patches)
            {
                patch.RevertPatch(patcher);
            }
        }

        private sealed class Patcher : IPatcher
        {
            private readonly HarmonyInstance harmony;

            public Patcher(HarmonyInstance harmony)
            {
                this.harmony = harmony;
            }

            public void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                if (prefixCall == null && postfixCall == null)
                {
                    throw new ArgumentException($"Both {nameof(prefixCall)} and {nameof(postfixCall)} cannot be null at the same time.");
                }

                harmony.Patch(method, new HarmonyMethod(prefixCall), new HarmonyMethod(postfixCall));
            }

            public void RevertPatch(MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                harmony.RemovePatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }
    }
}
