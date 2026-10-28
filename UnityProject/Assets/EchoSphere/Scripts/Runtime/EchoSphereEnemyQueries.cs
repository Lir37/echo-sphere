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
