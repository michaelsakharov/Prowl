﻿using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets
{
    [Importer("CSharpIcon.png", typeof(MonoScript), ".cs")]
    public class MonoScriptImporter : ScriptedImporter
    {
        static DateTime lastReload;

        public override void Import(SerializedAsset ctx, FileInfo assetPath)
        {
            ctx.SetMainObject(new MonoScript());

            if (lastReload == default)
                lastReload = DateTime.UtcNow;
            if (lastReload.AddSeconds(2) > DateTime.UtcNow)
                return;

            EditorApplication.Instance.RegisterReloadOfExternalAssemblies();

            lastReload = DateTime.UtcNow;

            ImGuiNotify.InsertNotification("Scripts Reloaded.", new(0.75f, 0.35f, 0.20f, 1.00f), assetPath.FullName);
        }
    }

}
