using Prowl.Runtime;
using HexaEngine.ImGuiNET;
using System.Threading.Channels;

namespace Prowl.Editor.PropertyDrawers;

public class PropertyDrawerColor : PropertyDrawer<Color> {

    protected override bool Draw(string label, ref Color value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();
        
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        
        System.Numerics.Vector4 v4 = value;
        changed = ImGui.ColorEdit4("", ref v4);
        value = new Color(v4.X, v4.Y, v4.Z, v4.W);
        
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
}

public class PropertyDrawerColor32 : PropertyDrawer<Color32> {

    protected override bool Draw(string label, ref Color32 value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();
        
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        
        System.Numerics.Vector4 v4 = new System.Numerics.Vector4(value.red / 255f, value.green / 255f, value.blue / 255f, value.alpha / 255f);
        changed = ImGui.ColorEdit4("", ref v4);
        value = new Color32((byte)(v4.X * 255f), (byte)(v4.Y * 255f), (byte)(v4.Z * 255f), (byte)(v4.W * 255f));
        
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
}
