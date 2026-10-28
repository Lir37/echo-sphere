using EchoSphere.Core;

internal static class CollisionScratchChecks
{
    internal static int Run()
    {
        var random=new Random(771028);
        var fresh=new List<CollisionBody>();var retained=new List<CollisionBody>();
        var towers=new List<Vec2>();var workspace=new SpaceCollisionRules.Workspace();
        int checks=0;
        void Require(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException("collision scratch: "+why);}
        for(int frame=0;frame<240;frame++)
        {
            fresh.Clear();retained.Clear();towers.Clear();
            int count=frame%8==0?0:frame%73+1;
            for(int i=0;i<count;i++)
            {
                var p=new Vec2((float)(random.NextDouble()*2400-1200),(float)(random.NextDouble()*2400-1200));
                float radius=8+i%40,speed=50+i%130,mass=1+i%9;bool breach=i%3==0;
                fresh.Add(new CollisionBody {Position=p,Radius=radius,Speed=speed,Mass=mass,FollowBreach=breach});
                retained.Add(new CollisionBody {Position=p,Radius=radius,Speed=speed,Mass=mass,FollowBreach=breach});
            }
            for(int i=0;i<frame%11;i++)towers.Add(new Vec2((float)(random.NextDouble()*600-300),(float)(random.NextDouble()*600-300)));
            var player=new Vec2(frame%7*20,frame%9*30);
            var a=SpaceCollisionRules.Resolve(player,fresh,towers,.016f,frame%2==0);
            var b=SpaceCollisionRules.ResolveRetained(player,retained,towers,.016f,frame%2==0,workspace);
            Require(a.X==b.X&&a.Y==b.Y,"player changed");
            for(int i=0;i<count;i++)Require(fresh[i].Position.X==retained[i].Position.X&&fresh[i].Position.Y==retained[i].Position.Y,"body order/position changed");
        }
        // A steady, warmed scene measures scratch allocation, not game FPS.
        for(int i=0;i<20;i++)SpaceCollisionRules.ResolveRetained(Vec2.Zero,retained,towers,.016f,false,workspace);
        long start=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100;i++)SpaceCollisionRules.ResolveRetained(Vec2.Zero,retained,towers,.016f,false,workspace);
        long pooled=GC.GetAllocatedBytesForCurrentThread()-start;
        start=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100;i++)SpaceCollisionRules.Resolve(Vec2.Zero,fresh,towers,.016f,false);
        long baseline=GC.GetAllocatedBytesForCurrentThread()-start;
        Require(pooled<baseline/4,"scratch allocation did not fall");
        Console.WriteLine($"Collision scratch (.NET fixture, 100 warmed calls): {baseline} -> {pooled} allocated bytes; exact comparisons={checks}.");
        return checks;
    }
}
