using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace PredPreySim.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
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
        public uint age;

        [FieldOffset(24)]
        public int state;

        [FieldOffset(28)]
        public int nnOffset;

        [FieldOffset(32)]
        public int meals;

        [FieldOffset(36)]
        public int deaths;

        [FieldOffset(40)]
        public float energySpent;

        [FieldOffset(44)]
        public int flag;

        public double Fitness()
        {
            var value = meals * 2 - deaths * 5 - energySpent * 0.01;
            return value * Math.Exp(-age / 10000);
        }
    }
}
