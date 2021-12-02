using System.Collections.Generic;
using UnityEngine;

namespace BadCode.Main
{
    public struct StartComponent
    {
        public GameObject loading_go;
        public Dictionary<StartProcess, float> process_dic;
    }
}