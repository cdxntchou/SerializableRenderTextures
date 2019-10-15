using UnityEditor;
using UnityEngine;

namespace Environment
{
    interface IChannel
    {
        GUID guid { get; }
        string description { get; }
    };

    class HeightMapChannel : IChannel
    {
        public GUID guid { get { return new GUID(); } }
        public string description { get { return "Heightmap"; } }
    };


    class Ecotope : UnityEngine.Object		//, IChannel
    {
        public string EcotopeClientData;
    }
}


