using System;
using System.Collections.Generic;
using Veldrid;

namespace Prowl.Runtime
{
    internal struct ViewportCommand : RenderingCommand
    {
        public int Index; 
        public bool SetFull;
        public int X, Y, Z;
        public int Width, Height, Depth;

        readonly void RenderingCommand.ExecuteCommand(CommandList list, ref RenderState state)
        {
            if (SetFull)
            {   
                if (Index < 0)
                    list.SetFullViewports();
                else
                    list.SetFullViewport((uint)Index);

                return;
            }

            Viewport viewport = new Viewport(X, Y, Width, Height, Z, Depth);

            if (Index < 0)
            {
                for (uint i = 0; i < state.activeFramebuffer.ColorTargets.Count; i++)
                    list.SetViewport(i, viewport);
            }
            else
            {
                list.SetViewport((uint)Index, viewport);
            }
        }
    }
}