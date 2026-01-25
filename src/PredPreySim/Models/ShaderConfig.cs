using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PredPreySim.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct ShaderConfig
    {
        public ShaderConfig() { }

        [FieldOffset(0)]
        public int agentsCount = 10000;

        [FieldOffset(4)]
        public int width = 1920*1;

        [FieldOffset(8)]
        public int height = 1080*1;

        [FieldOffset(12)]
        public float dt = 0.1f;

        [FieldOffset(16)]
        public float t;
    }
}
