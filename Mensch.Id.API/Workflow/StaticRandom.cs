using System;

namespace Mensch.Id.API.Workflow
{
    public static class StaticRandom
    {
        public static Random Rng { get; } = new();
    }
}