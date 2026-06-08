//****************************************************************************
// File: FlowGraph.cs
// Author: Li Nan
// Date: 2024-05-15 12:00
// Version: 1.0
//****************************************************************************

namespace Vena
{
    public class FlowGraph
    {
        
        

    }

    /// <summary>
    /// A node in the flow graph.
    /// </summary>
    public abstract class FlowGraphNode<T> where T :FlowGraph
    {
        public readonly T Graph;
            
        protected FlowGraphNode(T graph)
        {
            Graph = graph;
        }
    }
    
    /// <summary>
    /// A node in the flow graph.
    /// </summary>
    public abstract class LogicNode<T> : FlowGraphNode<T> where T : FlowGraph
    {
        protected LogicNode(T graph) : base(graph)
        {
            
        }
        
        public abstract void BeforeExecute();
        
        public abstract void Execute();
        
        public abstract void AfterExecute();
    }
    
    /// <summary>
    ///  An edge in the flow graph. a condition from prev to next.
    /// </summary>
    public abstract class FlowEdge<TGraph, TPrev, TNext> : FlowGraphNode<TGraph> 
        where TGraph : FlowGraph 
        where TPrev : FlowGraphNode<TGraph>
        where TNext : FlowGraphNode<TGraph>
    {
        public readonly TPrev Prev;
        
        public readonly TNext Next;
        
        protected FlowEdge(TGraph graph, TPrev prev, TNext next) : base(graph)
        {
            Prev = prev;
            Next = next;
        }
        
        public abstract bool Condition();
    }
}