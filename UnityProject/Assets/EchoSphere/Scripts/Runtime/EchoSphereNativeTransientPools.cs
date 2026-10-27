using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Retirement is deferred until LateUpdate, matching Unity Destroy's
        // post-Update boundary. A birth cannot reuse an identity still observed
        // by another simulation phase or an in-flight hit callback this frame.
        private sealed class NativeTransientPools : IDisposable
        {
            private readonly EchoSphereRuntime _owner;
            private readonly ObjectPool<ProjectileAgent> _projectiles;
            private readonly ObjectPool<SourceParticle2D> _particles;
            private readonly HashSet<MonoBehaviour> _owned=new HashSet<MonoBehaviour>();
            private readonly List<MonoBehaviour> _pending=new List<MonoBehaviour>();
            private readonly HashSet<MonoBehaviour> _retiring=new HashSet<MonoBehaviour>();
            private readonly Predicate<MonoBehaviour> _removeRetired;
            internal NativeTransientPools(EchoSphereRuntime owner)
            {
                _owner=owner;
                _removeRetired=agent=>agent==null||_retiring.Contains(agent);
                _projectiles=new ObjectPool<ProjectileAgent>(()=>Create<ProjectileAgent>("Sphere Projectile"),null,p=>p.gameObject.SetActive(false),DestroyOwned,false,32,256);
                _particles=new ObjectPool<SourceParticle2D>(()=>{
                    var particle=Create<SourceParticle2D>("Source Particle");var renderer=particle.gameObject.AddComponent<SpriteRenderer>();renderer.sprite=RuntimeSpriteFactory.Disc;renderer.sortingOrder=24;return particle;
                },null,p=>p.gameObject.SetActive(false),DestroyOwned,false,32,220);
            }
            private T Create<T>(string name) where T:MonoBehaviour
            {var go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,_owner.gameObject.scene);var agent=go.AddComponent<T>();_owned.Add(agent);return agent;}
            private void DestroyOwned(MonoBehaviour agent){_owned.Remove(agent);if(agent!=null)Destroy(agent.gameObject);}
            private static T Activate<T>(ObjectPool<T> pool) where T:MonoBehaviour
            {T agent;do{agent=pool.Get();}while(agent==null);agent.transform.localScale=Vector3.one;agent.transform.rotation=Quaternion.identity;agent.gameObject.SetActive(true);return agent;}
            internal ProjectileAgent Projectile()=>Activate(_projectiles);
            internal SourceParticle2D Particle()=>Activate(_particles);
            internal bool Retire(MonoBehaviour agent)
            {if(!_owned.Contains(agent))return false;if(_retiring.Add(agent))_pending.Add(agent);return true;}
            internal void Flush()
            {
                if(_pending.Count>0){
                    // Compact once per list, preserving survivor order. A burst
                    // of expirations must not rescan every list for every item.
                    _owner._simulationAgents.RemoveAll(_removeRetired);
                    foreach(var phase in _owner._simulationTypes.Values)phase.RemoveAll(_removeRetired);
                }
                foreach(var agent in _pending){
                    if(agent==null){_owned.Remove(agent);continue;}
                    if(agent is ProjectileAgent projectile){projectile.ResetForNativePool();_projectiles.Release(projectile);}
                    else if(agent is SourceParticle2D particle){particle.ResetForNativePool();_particles.Release(particle);}
                }
                _pending.Clear();_retiring.Clear();
                // A save restore/scene cleanup may destroy an active object
                // outside this owner. Do not retain its managed wrapper.
                _owned.RemoveWhere(agent=>agent==null);
            }
            public void Dispose()
            {
                _pending.Clear();_retiring.Clear();_projectiles.Clear();_particles.Clear();
                foreach(var agent in _owned)if(agent!=null)Destroy(agent.gameObject);_owned.Clear();
            }
        }
        private NativeTransientPools _nativeTransientPools;
        private NativeTransientPools TransientPools=>_nativeTransientPools??(_nativeTransientPools=new NativeTransientPools(this));
        internal void RetireNativeTransient(MonoBehaviour agent)
        {if(_nativeTransientPools==null||!_nativeTransientPools.Retire(agent))Destroy(agent.gameObject);}
    }
}
