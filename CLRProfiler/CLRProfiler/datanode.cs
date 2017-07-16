using System;
using System.Collections;
using System.Collections.Generic;

namespace CLRProfiler
{	
    struct datanode
    {
        public int level { get; set; }

        public int id { get; set; }

        public string name { get; set; }
        
        public Int32 incl { get; set; }

        public Int32 excl { get; set; }

        public int timesBeenCalled { get; set; }

        public int timesMakeCalls { get; set; }

        public int category { get; set; }

        public Dictionary<Vertex, Edge> caller { get; set; }

        public Dictionary<Vertex, Edge> callee { get; set; }

        public Hashtable callerAlloc { get; set; }

        public Hashtable calleeAlloc { get; set; }
    }
}