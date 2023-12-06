using HexaEngine.ImGuiNET;
using Prowl.Runtime;

namespace Prowl.Editor.PropertyDrawers; 

public class PropertyDrawerDouble : PropertyDrawer<double> {

    protected override bool Draw(string label, ref double value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        changed = GUIHelper.DragDouble("", ref value, 0.01f);
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
    
}

public class PropertyDrawerFloat : PropertyDrawer<float> {

    protected override bool Draw(string label, ref float value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        changed = ImGui.DragFloat("", ref value, 0.01f, float.MinValue, float.MaxValue, "%g");
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
    
}

public class PropertyDrawerInt : PropertyDrawer<int> {

    protected override bool Draw(string label, ref int value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        changed = ImGui.DragInt("", ref value, 0.01f, int.MinValue, int.MaxValue, "%g");
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
    
}

public class PropertyDrawerShort : PropertyDrawer<short> {

    protected override bool Draw(string label, ref short value)
    {
        bool changed = false;
        ImGui.Columns(2);
        ImGui.Text(label);
        ImGui.SetColumnWidth(0, 70);
        ImGui.NextColumn();

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        int valInt = value;
        changed = ImGui.DragInt("", ref valInt, 0.01f, short.MinValue, short.MaxValue, "%g");
        value = (short)valInt;
        ImGui.PopItemWidth();
        ImGui.Columns(1);
        return changed;
    }
}