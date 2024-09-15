using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spark2D {
    public class ComputeUtil {
        Dictionary<int, Stack<ComputeBuffer>> _bufferPool = new Dictionary<int, Stack<ComputeBuffer>>();
        
        public static ComputeBuffer CreateBuffer(Type type, Array data) {
            var buffer = new ComputeBuffer(data.Length, System.Runtime.InteropServices.Marshal.SizeOf(type));
            buffer.SetData(data);
            return buffer;
        }
    }
}