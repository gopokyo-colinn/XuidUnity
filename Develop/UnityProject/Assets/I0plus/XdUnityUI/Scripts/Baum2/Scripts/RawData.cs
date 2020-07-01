using System.Collections.Generic;
using UnityEngine;

namespace Baum2
{
    public class RawData : MonoBehaviour
    {
        public RawData()
        {
            Info = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Info { get; }
    }
}