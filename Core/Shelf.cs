using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spark2D {
    [Serializable]
    public class ShelfRow {
        public string name;
        public Object[] items;
    } 
    
    [CreateAssetMenu(fileName = "Shelf", menuName = "Spark2D/Shelf", order = 0)]
    public class Shelf : ScriptableObject {
        public ShelfRow[] rows;

        public Object[] GetItems(string rowName) {
            foreach (var shelfRow in rows) {
                if (shelfRow.name == rowName) {
                    return shelfRow.items;
                }
            }
            return null;
        }
    }
}