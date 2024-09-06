using System;
using UnityEngine;

namespace Spark2D {
    #region Extras
    [Serializable]
    public class ObjectMappingPair {
        public UnityEngine.Object obj;
        public string name;

        public ObjectMappingPair(UnityEngine.Object obj, string m) {
            this.obj = obj;
            this.name = m;
        }
    }
    #endregion

    [CreateAssetMenu(fileName = "Depot", menuName = "Spark2D/Depot", order = 0)]
    public class Depot : ScriptableObject {
        [PairMapping("obj", "name")]
        public ObjectMappingPair[] contents;

        public object Get(string name) {
            foreach (var pair in contents) {
                if (pair.name == name) {
                    return pair.obj;
                }
            }
            return null;
        }
    }
}