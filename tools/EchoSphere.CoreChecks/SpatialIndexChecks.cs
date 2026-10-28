using EchoSphere.Core;

internal static class SpatialIndexChecks
{
    private sealed class Item {internal float X,Y;}
    internal static int Run()
    {
        int checks=0;var random=new Random(271028);var index=new OrderedSpatialIndex<Item>(2.3f);
        var order=new List<Item>();var candidates=new List<Item>();
        void Require(bool value,string reason){checks++;if(!value)throw new InvalidOperationException("spatial: "+reason);}
        float Number(float scale)=>(float)(random.NextDouble()*2-1)*scale;
        float Distance(Item item,float x,float y){float dx=item.X-x,dy=item.Y-y;return dx*dx+dy*dy;}
        for(int i=0;i<160;i++){var item=new Item {X=Number(40),Y=Number(40)};order.Add(item);index.Upsert(item,item.X,item.Y);}
        order[0].X=order[1].X=2.3f;order[0].Y=order[1].Y=0;index.Upsert(order[0],2.3f,0);index.Upsert(order[1],2.3f,0);
        for(int step=0;step<1800;step++){
            if(step%3==0){var item=order[random.Next(order.Count)];item.X=Number(60);item.Y=Number(60);index.Upsert(item,item.X,item.Y);}
            if(step%17==0){int at=random.Next(order.Count);var item=order[at];index.Remove(item);order.RemoveAt(at);index.Upsert(item,item.X,item.Y);order.Add(item);}
            float x=step%11==0?0:Number(45),y=step%11==0?0:Number(45);
            float radius=step%13==0?2.3f:step%29==0?float.PositiveInfinity:step%31==0?float.NaN:step%5==0?-Number(15):Math.Abs(Number(15));
            index.Candidates(x,y,radius,candidates);float squared=radius*radius;
            var expected=order.Where(item=>Distance(item,x,y)<=squared).ToArray();var actual=candidates.Where(item=>Distance(item,x,y)<=squared).ToArray();
            Require(expected.SequenceEqual(actual),"ordered radius membership");
            Item Reference(IEnumerable<Item> items){Item best=null;float distance=squared;foreach(var item in items){float d=Distance(item,x,y);if(d<distance){distance=d;best=item;}}return best;}
            Require(Reference(order)==Reference(candidates),"nearest/tie order");
        }
        var invalid=new Item {X=float.NaN,Y=float.PositiveInfinity};order.Add(invalid);index.Upsert(invalid,invalid.X,invalid.Y);
        index.Candidates(0,0,float.PositiveInfinity,candidates);Require(order.SequenceEqual(candidates),"nonfinite/unbounded order");
        index.Upsert(invalid,1,1);index.Candidates(1,1,0,candidates);Require(candidates.Contains(invalid),"invalid to valid position");
        index.Remove(invalid);index.Candidates(1,1,0,candidates);Require(!candidates.Contains(invalid),"remove");
        index.Clear();index.Candidates(0,0,100,candidates);Require(index.Count==0&&candidates.Count==0,"clear");
        for(int i=0;i<2000;i++){var item=new Item {X=i%50*5,Y=i/50*5};index.Upsert(item,item.X,item.Y);}
        index.Candidates(100,100,2,candidates);
        Require(candidates.Count>0&&candidates.Count<20,"localized query failed to exclude distant candidates");
        return checks;
    }
}
