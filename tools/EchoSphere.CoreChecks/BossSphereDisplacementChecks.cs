using EchoSphere.Core;

internal static class BossSphereDisplacementChecks
{
    internal static int Run()
    {
        int checks=0;
        void A(bool ok,string why){checks++;if(!ok)throw new Exception("Boss sphere push: "+why);}
        var work=new BossSphereDisplacementRules.Workspace();
        var boss=new CollisionBody{Position=new Vec2(-53,0),Radius=34,Speed=60,Mass=8,PushesSpheres=true,SpherePushMotion=new Vec2(1,0)};
        var bodies=new[]{boss};var spheres=new List<Vec2>{Vec2.Zero};
        A(BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work),"actual contact moves a sphere");
        A(spheres[0].X>0&&spheres[0].X<=2,"smooth displacement bounded by 120px/s");
        boss.PushesSpheres=false;spheres[0]=Vec2.Zero;
        A(!BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work)&&spheres[0].Equals(Vec2.Zero),"ordinary enemies cannot move spheres");
        boss.PushesSpheres=true;boss.SpherePushMotion=Vec2.Zero;
        A(!BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work),"stationary or frozen body does not push");
        boss.SpherePushMotion=new Vec2(-1,0);
        A(!BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work),"retreat does not drag spheres behind boss");
        boss.SpherePushMotion=new Vec2(1,0);boss.Position=new Vec2(-90,0);
        A(!BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work),"no force outside body contact");
        boss.Position=new Vec2(-53,0);
        A(!BossSphereDisplacementRules.Resolve(bodies,spheres,0,work),"paused simulation does not move nodes");
        spheres=new List<Vec2>{Vec2.Zero,new Vec2(56,0),new Vec2(112,0)};
        BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work);
        A(spheres[1].X>56&&spheres[2].X>112,"compression propagates through adjacent nodes");
        for(int i=1;i<spheres.Count;i++)A(Vec2.Distance(spheres[i],spheres[i-1])>=55.999f,"56px spacing preserved in a pushed chain");
        spheres=new List<Vec2>{new Vec2(1180,0)};boss.Position=new Vec2(1127,0);
        A(BossSphereDisplacementRules.Resolve(bodies,spheres,1f/60,work),"wall contact can slide sideways");
        A(Math.Abs(spheres[0].X)<=1180&&Math.Abs(spheres[0].Y)<=1180&&Vec2.Distance(spheres[0],new Vec2(1180,0))<=2.0001f,"arena edge and movement budget");
        float[] ends=new float[3];int[] fps={30,60,120};
        for(int f=0;f<fps.Length;f++){
            spheres=new List<Vec2>{Vec2.Zero};boss.Position=new Vec2(-90,0);
            for(int frame=0;frame<fps[f]*3;frame++){
                boss.SpherePushMotion=new Vec2(60f/fps[f],0);boss.Position+=boss.SpherePushMotion;
                BossSphereDisplacementRules.Resolve(bodies,spheres,1f/fps[f],work);
            }
            ends[f]=spheres[0].X;
            A(boss.Position.X>50&&ends[f]>100,"boss crosses original node location without a barricade");
        }
        A(ends.Max()-ends.Min()<2.1f,"30/60/120Hz contact response stays close");
        spheres=new List<Vec2>{Vec2.Zero};boss.Position=new Vec2(-53,0);boss.SpherePushMotion=new Vec2(2,0);
        var two=new[]{boss,new CollisionBody{Position=boss.Position,Radius=34,PushesSpheres=true,SpherePushMotion=new Vec2(2,0)}};
        BossSphereDisplacementRules.Resolve(two,spheres,1f/60,work);
        A(spheres[0].Length<=2.0001f,"multiple bosses share one per-node frame budget");
        var player=SpaceCollisionRules.Resolve(new Vec2(300,300),bodies,spheres,.016f,false);
        A(Vec2.Distance(player,new Vec2(300,300))<.0001f&&boss.Position.X==-53,"boss is not expelled by the old immovable sphere collision");
        for(int i=0;i<20;i++){spheres[0]=Vec2.Zero;BossSphereDisplacementRules.Resolve(bodies,spheres,.016f,work);}
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100;i++){spheres[0]=Vec2.Zero;BossSphereDisplacementRules.Resolve(bodies,spheres,.016f,work);}
        A(GC.GetAllocatedBytesForCurrentThread()==before,"warmed contact solver does not allocate");
        return checks;
    }
}
