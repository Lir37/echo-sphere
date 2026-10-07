
import { useRef, useState, type CSSProperties, type PointerEvent as ReactPointerEvent } from 'react';
import { REGION_CHALLENGES, loadRegionChallengeCompletions, loadRegionEndlessUnlock, loadRegionStabilized, type RegionChallengeId, type RegionMode } from './region';
import type { Lang } from './i18n';

type StartRun = (mode: RegionMode, challenge: RegionChallengeId) => void;

const DISTANT_BODIES = [
  { x: 15, y: 27, size: 24, tone: '#4e8fb6' },
  { x: 82, y: 23, size: 18, tone: '#8170bb' },
  { x: 84, y: 74, size: 30, tone: '#3e7894' },
  { x: 20, y: 75, size: 16, tone: '#617ba8' },
  { x: 66, y: 13, size: 9, tone: '#709fc4' },
] as const;

const CHALLENGE_CLASS: Record<RegionChallengeId, string> = {
  none: '',
  fractured_network: 'challenge-fracture',
  overload: 'challenge-overload',
  low_gravity: 'challenge-gravity',
};

export default function EchoMapScreen({ lang, onStartRun, onBack }: {
  lang: Lang;
  onStartRun: StartRun;
  onBack: () => void;
}) {
  const [regionOpen,setRegionOpen]=useState(false);
  const [pan,setPan]=useState({x:0,y:0});
  const drag=useRef({active:false,pointerId:-1,startX:0,startY:0,originX:0,originY:0});

  const completed=loadRegionChallengeCompletions();
  const stabilized=loadRegionStabilized();
  const endlessUnlocked=loadRegionEndlessUnlock();

  const beginPan=(event:ReactPointerEvent<HTMLDivElement>)=>{
    const target=event.target;
    if(target instanceof Element && target.closest('button'))return;
    drag.current={active:true,pointerId:event.pointerId,startX:event.clientX,startY:event.clientY,originX:pan.x,originY:pan.y};
    event.currentTarget.setPointerCapture(event.pointerId);
  };
  const movePan=(event:ReactPointerEvent<HTMLDivElement>)=>{
    if(!drag.current.active||drag.current.pointerId!==event.pointerId)return;
    event.preventDefault();
    setPan({
      x:Math.max(-300,Math.min(300,drag.current.originX+event.clientX-drag.current.startX)),
      y:Math.max(-220,Math.min(220,drag.current.originY+event.clientY-drag.current.startY)),
    });
  };
  const endPan=(event:ReactPointerEvent<HTMLDivElement>)=>{
    if(drag.current.pointerId===event.pointerId)drag.current.active=false;
  };
  const closeRegion=()=>{setRegionOpen(false);setPan({x:0,y:0});};

  return (
    <div className={'es-map-screen '+(regionOpen?'is-region-open':'is-region-overview')}>
      <header className="es-map-header">
        <button type="button" className="es-map-back" onClick={()=>regionOpen?closeRegion():onBack()} aria-label={regionOpen?(lang==='ru'?'Назад к звёздам':'Back to stars'):(lang==='ru'?'В меню':'Back to menu')}>
          <span>‹</span>{regionOpen?(lang==='ru'?'ЗВЁЗДНАЯ КАРТА':'STAR MAP'):(lang==='ru'?'МЕНЮ':'MENU')}
        </button>
        <div className="es-map-title-cluster">
          <div className="es-map-title">{lang==='ru'?'КАРТА ЭХА':'ECHO MAP'}</div>
          <div className="es-map-subtitle">{lang==='ru'?'ПОЛЕ СИГНАЛОВ':'SIGNAL FIELD'}</div>
        </div>
      </header>

      <main className="es-map-main" onPointerDown={beginPan} onPointerMove={movePan} onPointerUp={endPan} onPointerCancel={endPan}>
        <div className="es-map-space">
          <div className="es-map-stars"/>
          <div className="es-map-nebula nebula-a"/>
          <div className="es-map-nebula nebula-b"/>

          <div className="es-map-camera" style={{transform:`translate3d(${pan.x}px,${pan.y}px,0) scale(${regionOpen?1.70:1})`}}>
            <div className="es-map-star-lines" aria-hidden="true"><span/><span/><span/><span/><span/><span/></div>
            {DISTANT_BODIES.map((body,index)=><span key={index} className="es-map-distant-region" style={{left:body.x+'%',top:body.y+'%','--body-size':body.size+'px','--body-tone':body.tone} as CSSProperties}/>)}

            <div className="es-map-region-cluster" style={{left:'50%',top:'50%'}}>
              <div className="es-map-region-orbit-track" aria-hidden="true">
                <span className={'es-map-orbit-satellite challenge-fracture '+(completed.includes('fractured_network')?'is-complete':'is-locked')}/>
                <span className={'es-map-orbit-satellite challenge-overload '+(completed.includes('overload')?'is-complete':'is-locked')}/>
                <span className={'es-map-orbit-satellite challenge-gravity '+(completed.includes('low_gravity')?'is-complete':'is-locked')}/>
              </div>

              <button type="button" className="es-map-region-body" onClick={()=>regionOpen?onStartRun('stabilization','none'):setRegionOpen(true)} aria-label={regionOpen?(lang==='ru'?'Начать стандартную стабилизацию':'Start Standard Stabilization'):(lang==='ru'?'Приблизить Резонансный бассейн':'Approach Resonance Basin')}>
                <span className="es-map-region-glow"/>
                <span className="es-map-region-surface"/>
                <span className="es-map-region-core"/>
                <span className="es-map-region-ring ring-a"/>
                <span className="es-map-region-ring ring-b"/>
              </button>

              <div className="es-map-region-actions">
                {REGION_CHALLENGES.map(challenge=>{
                  const done=completed.includes(challenge.id);
                  const locked=!stabilized;
                  const cls=challenge.id==='none'?'':CHALLENGE_CLASS[challenge.id];
                  return <button key={challenge.id} type="button" disabled={locked}
                    className={'es-map-orbit-action '+cls+(done?' is-complete':'')+(locked?' is-locked':'')}
                    onClick={()=>!locked&&onStartRun('stabilization',challenge.id)}
                    aria-label={locked?(lang==='ru'?challenge.name.ru+': сначала стандартный забег':challenge.name.en+': clear the base run first'):challenge.name[lang]}>
                    <span className="es-map-challenge-icon"/>
                  </button>;
                })}
              </div>

              <button type="button" disabled={!endlessUnlocked} className={'es-map-endless-node '+(endlessUnlocked?'is-unlocked':'is-locked')}
                onClick={()=>endlessUnlocked&&onStartRun('endless','none')}
                aria-label={endlessUnlocked?(lang==='ru'?'Бесконечность':'Endless'):(lang==='ru'?'Бесконечность закрыта':'Endless locked')}>
                <span className="es-map-endless-symbol"/>
              </button>
            </div>
          </div>
          <div className="es-map-pan-hint" aria-hidden="true">{lang==='ru'?'СВАЙП ДЛЯ ПЕРЕМЕЩЕНИЯ':'DRAG TO PAN'}</div>
        </div>
      </main>
    </div>
  );
}
