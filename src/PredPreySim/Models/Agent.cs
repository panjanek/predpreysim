using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace PredPreySim.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Agent
    {
        [FieldOffset(0)]
        public Vector2 position;

        [FieldOffset(8)]
        public float angle;

        [FieldOffset(12)]
        public int type;

        [FieldOffset(16)]
        public float energy;

        [FieldOffset(20)]
        public float age;

        [FieldOffset(24)]
        public int state;

        [FieldOffset(28)]
        public int pad;
    }
}
