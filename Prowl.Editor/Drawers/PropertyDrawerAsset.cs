﻿using Hexa.NET.ImGui;
using Prowl.Editor.Assets;
using Prowl.Editor.ImGUI.Widgets;
using Prowl.Icons;
using Prowl.Runtime;

namespace Prowl.Editor.PropertyDrawers;

public class PropertyDrawerAsset : PropertyDrawer<IAssetRef>
{
    public static PropertyDrawerAsset Selected;

    public virtual string Name { get; } = FontAwesome6.Circle;

    protected override bool Draw(string label, ref IAssetRef value, float width)
    {
        bool changed = false;
        DrawLabel(label, ref width);

        string path;
        if (value.IsExplicitNull) {
            path = "(Null)";
            if (ImGui.Selectable($"{Name}: {path}", Selected == this, new System.Numerics.Vector2(width, 17))) {
                Selected = this;
#warning TODO: Show a popup with a list of all assets of the type - property.Type.Name
            }
            GUIHelper.ItemRectFilled(0.9f, 0.1f, 0.1f, 0.3f);
        } else if (value.IsRuntimeResource) {
            path = "(Runtime)" + value.Name;
            if (ImGui.Selectable($"{Name}: {path}", Selected == this, new System.Numerics.Vector2(width, 17))) {
                Selected = this;
#warning TODO: Show a popup with a list of all assets of the type - property.Type.Name
            }
            GUIHelper.ItemRectFilled(0.1f, 0.1f, 0.9f, 0.3f);
        } else if (AssetDatabase.TryGetFile(value.AssetID, out var assetPath)) {
            path = AssetDatabase.ToRelativePath(assetPath);
            if (ImGui.Selectable($"{Name}: {path}", Selected == this, new System.Numerics.Vector2(width, 17))) {
                AssetDatabase.Ping(value.AssetID);
                Selected = this;
            }
        }

        if (value.IsAvailable && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            GlobalSelectHandler.Select(value);

        // DragDrop code
        string payloadName = value.InstanceType.Name;
        if (value.InstanceType.IsAssignableTo(typeof(ScriptableObject)))
            payloadName = "ScriptableObject"; // Scriptable objects are a special case
        if (DragnDrop.ReceiveAsset(out Guid assetGuid, payloadName)) {
            value.AssetID = assetGuid;
            changed = true;
        }

        // Add a button for clearing the Asset
        if (ImGui.IsKeyPressed(ImGuiKey.Delete) && ImGui.IsWindowFocused()) {
            if (Selected == this) {
                value.AssetID = Guid.Empty;
                value.FileID = 0;
                changed = true;
            }
        }
        ImGui.Columns(1);
        return changed;
    }
}
