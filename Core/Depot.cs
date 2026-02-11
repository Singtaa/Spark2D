using System;
using UnityEngine;

namespace Spark2D {
    #region Extras
    [Serializable]
    public class ObjectMappingPair {
        public string name;
        public UnityEngine.Object obj;

        public ObjectMappingPair(string m, UnityEngine.Object obj) {
            this.obj = obj;
            this.name = m;
        }
    }
    #endregion

    [CreateAssetMenu(fileName = "Depot", menuName = "Spark2D/Depot", order = 0)]
    public class Depot : ScriptableObject {
        [PairMapping("name", "obj")]
        public ObjectMappingPair[] contents;

        public UnityEngine.Object Get(string name) {
            foreach (var pair in contents) {
                if (pair.name == name) {
                    return pair.obj;
                }
            }
            return null;
        }
    }
}