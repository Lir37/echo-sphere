using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    // Broad phase only. Callers apply their original exact predicates and tie
    // rules. Results follow insertion order even after moving between cells.
    internal sealed class OrderedSpatialIndex<T> where T : class
    {
        private readonly struct Cell : IEquatable<Cell>
        {
            internal readonly int X,Y;
            internal Cell(int x,int y){X=x;Y=y;}
            public bool Equals(Cell value)=>X==value.X&&Y==value.Y;
            public override bool Equals(object value)=>value is Cell other&&Equals(other);
            public override int GetHashCode()=>HashCode.Combine(X,Y);
        }
        private sealed class Entry {internal T Item;internal Cell Cell;internal bool Indexed,Removed;internal long Order;}
        private sealed class ByOrder : IComparer<Entry> {internal static readonly ByOrder Instance=new ByOrder();public int Compare(Entry a,Entry b)=>a.Order.CompareTo(b.Order);}
        private readonly double _size;
        private readonly Dictionary<T,Entry> _entries=new Dictionary<T,Entry>();
        private readonly Dictionary<Cell,List<Entry>> _cells=new Dictionary<Cell,List<Entry>>();
        private readonly Stack<List<Entry>> _freeCells=new Stack<List<Entry>>();
        private readonly List<Entry> _order=new List<Entry>(),_query=new List<Entry>();
        private int _removed,_unindexed;private long _next;
        internal int Count=>_entries.Count;
        internal OrderedSpatialIndex(float cellSize){if(!float.IsFinite(cellSize)||cellSize<=0)throw new ArgumentOutOfRangeException(nameof(cellSize));_size=cellSize;}
        internal bool Contains(T item)=>_entries.ContainsKey(item);
        private bool Locate(double x,double y,out Cell cell)
        {x=Math.Floor(x/_size);y=Math.Floor(y/_size);if(!double.IsFinite(x+y)||x<int.MinValue||x>int.MaxValue||y<int.MinValue||y>int.MaxValue){cell=default;return false;}cell=new Cell((int)x,(int)y);return true;}
        private void Detach(Entry entry)
        {
            if(!entry.Indexed)return;
            var list=_cells[entry.Cell];list.Remove(entry);if(list.Count==0){_cells.Remove(entry.Cell);if(_freeCells.Count<256)_freeCells.Push(list);}entry.Indexed=false;
        }
        internal void Upsert(T item,float x,float y)
        {
            if(!_entries.TryGetValue(item,out var entry)){entry=new Entry {Item=item,Order=_next++};_unindexed++;_entries.Add(item,entry);_order.Add(entry);}
            bool valid=Locate(x,y,out var cell);if(valid&&entry.Indexed&&cell.Equals(entry.Cell))return;bool wasIndexed=entry.Indexed;Detach(entry);if(wasIndexed)_unindexed++;
            if(!valid)return;
            if(!_cells.TryGetValue(cell,out var list)){list=_freeCells.Count>0?_freeCells.Pop():new List<Entry>();_cells.Add(cell,list);}
            entry.Cell=cell;entry.Indexed=true;_unindexed--;list.Add(entry);
        }
        internal void Remove(T item)
        {
            if(!_entries.TryGetValue(item,out var entry))return;if(!entry.Indexed)_unindexed--;Detach(entry);_entries.Remove(item);entry.Item=null;entry.Removed=true;_removed++;
            if(_removed>=256&&_removed>_order.Count/4){_order.RemoveAll(e=>e.Removed);_removed=0;}
        }
        internal void Clear()
        {
            foreach(var list in _cells.Values){list.Clear();if(_freeCells.Count<256)_freeCells.Push(list);}
            _cells.Clear();_entries.Clear();_order.Clear();_query.Clear();_removed=0;_unindexed=0;_next=0;
        }
        internal void Candidates(float x,float y,float radius,List<T> result)
        {
            result.Clear();double r=Math.Abs((double)radius);
            // Conservative padding covers float rounding in callers' squared
            // distance predicates at cell/range boundaries.
            r+=(Math.Abs((double)x)+Math.Abs((double)y)+r+1)*.000001;
            var min=default(Cell);var max=default(Cell);
            bool minimum=Locate(x-r,y-r,out min),maximum=Locate(x+r,y+r,out max);bool full=!minimum||!maximum;
            if(!full){long columns=(long)max.X-min.X+1,rows=(long)max.Y-min.Y+1;full=columns<=0||rows<=0||columns>int.MaxValue||rows>int.MaxValue||columns*rows>Math.Max(16L,(long)_cells.Count*2);}
            if(full){foreach(var entry in _order)if(!entry.Removed)result.Add(entry.Item);return;}
            _query.Clear();
            for(long cy=min.Y;cy<=max.Y;cy++)for(long cx=min.X;cx<=max.X;cx++)if(_cells.TryGetValue(new Cell((int)cx,(int)cy),out var list))_query.AddRange(list);
            // Non-finite positions must retain their original predicate behavior.
            if(_unindexed>0)foreach(var entry in _order)if(!entry.Removed&&!entry.Indexed)_query.Add(entry);
            _query.Sort(ByOrder.Instance);foreach(var entry in _query)result.Add(entry.Item);
        }
    }
}
