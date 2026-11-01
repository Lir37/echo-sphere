using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Generic engine-object fields are excluded by the existing snapshot
        // codec. The scalar radius bound is explicitly in NonRunFields.
        private OrderedSpatialIndex<EnemyAgent2D> _enemyQueries;
        private readonly List<EnemyAgent2D> _enemyQueryCandidates=new List<EnemyAgent2D>();
        private float _maximumEnemyQueryRadius;
        private int _enemyQueryListCount=-1;
        private void RegisterEnemyQuery(EnemyAgent2D enemy)
        {
            UpdateEnemyQuery(enemy);
            _enemyQueryListCount=_enemies.Count;
        }
        private void UpdateEnemyQuery(EnemyAgent2D enemy)
        {
            if(_enemyQueries==null||enemy==null)return;
            var p=enemy.transform.position;_enemyQueries.Upsert(enemy,p.x,p.y);
            _maximumEnemyQueryRadius=Mathf.Max(_maximumEnemyQueryRadius,enemy.SourceRadius*SpherePlacementRules.SourcePixelsToWorld);
        }
        internal void RefreshEnemyQuery(EnemyAgent2D enemy)
        {if(_enemyQueries!=null&&enemy!=null&&_enemyQueries.Contains(enemy))UpdateEnemyQuery(enemy);}
        internal void RemoveEnemyQuery(EnemyAgent2D enemy){if(_enemyQueries!=null)_enemyQueries.Remove(enemy);}
        // Local separation uses the retained spatial index, not an all-enemy scan.
        internal Vector2 SteerRegionalEnemy(EnemyAgent2D self,Vector2 desired){
            if(desired.sqrMagnitude<.001f)return desired;
            Vector2 position=self.transform.position,avoid=Vector2.zero;
            float radius=self.SourceRadius*SpherePlacementRules.SourcePixelsToWorld;
            var nearby=EnemyQuery(position,radius*3,true);
            foreach(var other in nearby){if(other==null||other==self||!other.IsAlive)continue;Vector2 delta=position-(Vector2)other.transform.position;float d=delta.magnitude,space=radius+other.SourceRadius*SpherePlacementRules.SourcePixelsToWorld;
                if(d>.001f&&d<space*1.4f)avoid+=delta/d*(1-d/(space*1.4f));}
            return Vector2.ClampMagnitude(desired+avoid*.85f,1);
        }
        private List<EnemyAgent2D> EnemyQuery(Vector2 center,float radius,bool includeEnemyRadius=false)
        {
            // Destroy removes an index entry before the source list's end-frame
            // cleanup. Compare the observed list count, not live index entries,
            // to avoid rebuilding for every query during that interval.
            if(_enemyQueries==null||_enemyQueryListCount!=_enemies.Count){
                _enemyQueries??=new OrderedSpatialIndex<EnemyAgent2D>(128f*SpherePlacementRules.SourcePixelsToWorld);
                _enemyQueries.Clear();_maximumEnemyQueryRadius=0;foreach(var enemy in _enemies)UpdateEnemyQuery(enemy);_enemyQueryListCount=_enemies.Count;
            }
            float bound=Mathf.Abs(radius)+(includeEnemyRadius?_maximumEnemyQueryRadius:0);
            _enemyQueries.Candidates(center.x,center.y,bound,_enemyQueryCandidates);return _enemyQueryCandidates;
        }
    }
}
