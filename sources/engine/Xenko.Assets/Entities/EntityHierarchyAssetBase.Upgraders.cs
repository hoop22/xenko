﻿// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xenko.Core.Assets;
using Xenko.Core;
using Xenko.Core.Extensions;
using Xenko.Core.Yaml;
using Xenko.Core.Yaml.Serialization;

namespace Xenko.Assets.Entities
{
    partial class EntityHierarchyAssetBase
    {
        /// <summary>
        /// Moves Group from Entity to inside components (for those that support it)
        /// </summary>
        protected class MoveRenderGroupInsideComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;
                foreach (dynamic entityDesign in entities)
                {
                    var entity = entityDesign.Entity;

                    // Check if entity has a group (otherwise nothing to do
                    var group = entity.Group;
                    if (group == null)
                        continue;

                    // Save override and remove old element
                    var groupOverride = entity.GetOverride("Group");
                    entity.RemoveChild("Group");

                    foreach (var component in entity.Components)
                    {
                        try
                        {
                            var componentTag = component.Value.Node.Tag;
                            if (componentTag == "!ModelComponent"
                                || componentTag == "!SpriteComponent" || componentTag == "!UIComponent"
                                || componentTag == "!BackgroundComponent" || componentTag == "!SkyboxComponent"
                                || componentTag == "!ParticleSystemComponent"
                                || componentTag == "!SpriteStudioComponent")
                            {
                                component.Value.RenderGroup = group;
                                component.Value.SetOverride("RenderGroup", groupOverride);
                            }
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Gravity field on all CharacterComponents in a SceneAsset from float to Vector3 to support three-dimensional gravity.
        /// </summary>
        protected class CharacterComponentGravityVector3Upgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                // Set up asset and entity hierarchy for reading.
                var hierarchy = asset.Hierarchy;
                var entities = (DynamicYamlArray)hierarchy.Parts;

                // Loop through the YAML file.
                foreach (dynamic entityDesign in entities)
                {
                    // Get the entity.
                    var entity = entityDesign.Entity;

                    // Further loop to find CharacterComponents to upgrade.
                    foreach (var component in entity.Components)
                    {
                        try
                        {
                            var componentTag = component.Value.Node.Tag;

                            // Is this a character component?
                            if (componentTag == "!CharacterComponent")
                            {
                                // Retrieve old gravity value.
                                var oldGravity = component.Value.Gravity as DynamicYamlScalar;

                                //Actually upgrade Gravity to a Vector3.
                                if (component.Value.ContainsChild("Gravity"))
                                {
                                    component.Value.Gravity = new YamlMappingNode
                                    {
                                        { new YamlScalarNode("X"), new YamlScalarNode("0.0") },
                                        { new YamlScalarNode("Y"), new YamlScalarNode(oldGravity.Node.Value) },
                                        { new YamlScalarNode("Z"), new YamlScalarNode("0.0") }
                                    };
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }
        }
    }
}
