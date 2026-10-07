import { useRef, useState, type CSSProperties, type PointerEvent as ReactPointerEvent } from 'react';
import { REGION_CONFIGS, getRegionConfig, isRegionAvailable, loadRegionChallengeCompletions, loadRegionEndlessUnlock, loadRegionStabilized, type RegionChallengeId, type RegionId, type RegionMode } from './region';
import type { Lang } from './i18n';

type StartRun=(regionId:RegionId,mode:RegionMode,challenge:RegionChallengeId)=>void;
const BODY_POSITIONS:Record<RegionId,{x:number;y:number;size:number}>={resonance_basin:{x:34,y:50,size:44},spectral_rift:{x:68,y:39,size:38}};
const CHALLENGE_CLASS:Record<string,string>={fractured_network:'challenge-fracture',overload:'challenge-overload',low_gravity:'challenge-gravity',mirror_tide:'challenge-mirror',volatile_core:'challenge-volatile',phase_drift:'challenge-phase'};

export default function EchoMapScreen({lang,onStartRun,onBack}:{lang:Lang;onStartRun:StartRun;onBack:()=>void}){
 const [selected,setSelected]=useState<RegionId|null>(null); const [pan,setPan]=useState({x:0,y:0});
 const drag=useRef({active:false,pointerId:-1,startX:0,startY:0,originX:0,originY:0});
 const beginPan=(e:ReactPointerEvent<HTMLDivElement>)=>{const t=e.target;if(t instanceof Element&&t.closest('button'))return;drag.current={active:true,pointerId:e.pointerId,startX:e.clientX,startY:e.clientY,originX:pan.x,originY:pan.y};e.currentTarget.setPointerCapture(e.pointerId)};
 const movePan=(e:ReactPointerEvent<HTMLDivElement>)=>{if(!drag.current.active||drag.current.pointerId!==e.pointerId)return;e.preventDefault();setPan({x:Math.max(-340,Math.min(340,drag.current.originX+e.clientX-drag.current.startX)),y:Math.max(-240,Math.min(240,drag.current.originY+e.clientY-drag.current.startY))})};
 const endPan=(e:ReactPointerEvent<HTMLDivElement>)=>{if(drag.current.pointerId===e.pointerId)drag.current.active=false};
 const focused=selected?getRegionConfig(selected):null; const close=()=>{setSelected(null);setPan({x:0,y:0})};
 return <div className={'es-map-screen '+(selected?'is-region-open':'is-region-overview')}>
  <header className="es-map-header"><button type="button" className="es-map-back" onClick={()=>selected?close():onBack()} aria-label={selected?(lang==='ru'?'Назад к звёздам':'Back to stars'):(lang==='ru'?'В меню':'Back to menu')}><span>‹</span>{selected?(lang==='ru'?'ЗВЁЗДНАЯ КАРТА':'STAR MAP'):(lang==='ru'?'МЕНЮ':'MENU')}</button><div className="es-map-title-cluster"><div className="es-map-title">{lang==='ru'?'КАРТА ЭХА':'ECHO MAP'}</div><div className="es-map-subtitle">{selected?(lang==='ru'?'РЕГИОН В ФОКУСЕ':'REGION FOCUS'):(lang==='ru'?'ЗВЁЗДНОЕ ПОЛЕ':'STAR FIELD')}</div></div></header>
  <main className="es-map-main" onPointerDown={beginPan} onPointerMove={movePan} onPointerUp={endPan} onPointerCancel={endPan}><div className="es-map-space"><div className="es-map-stars"/><div className="es-map-nebula nebula-a"/><div className="es-map-nebula nebula-b"/>
   <div className="es-map-camera" style={{transform:'translate3d('+pan.x+'px,'+pan.y+'px,0) scale('+(selected?1.72:1)+')}}><div className="es-map-star-lines" aria-hidden="true"/>
    {!selected&&REGION_CONFIGS.map(cfg=>{const pos=BODY_POSITIONS[cfg.id],available=isRegionAvailable(cfg.id);return <button key={cfg.id} type="button" className={'es-map-region-body es-map-overview-body '+(available?'is-available':'is-locked')} style={{left:pos.x+'%',top:pos.y+'%','--region-accent':cfg.accent,'--region-size':pos.size+'px'} as CSSProperties} onClick={()=>available&&setSelected(cfg.id)} aria-label={available?cfg.name[lang]:(lang==='ru'?'Регион закрыт':'Region locked')}><span className="es-map-region-glow"/><span className="es-map-region-surface"/><span className="es-map-region-core"/><span className="es-map-region-ring ring-a"/><span className="es-map-region-ring ring-b"/><span className="es-map-body-orbit"><i/><i/><i/></span></button>})}
    {selected&&focused&&<div className="es-map-region-cluster" style={{left:'50%',top:'50%'}}><div className="es-map-region-orbit-track" aria-hidden="true"/>
      {focused.challenges.map(challenge=>{const done=loadRegionChallengeCompletions(selected).includes(challenge.id),locked=!loadRegionStabilized(selected);return <button key={challenge.id} type="button" disabled={locked} className={'es-map-orbit-action '+(CHALLENGE_CLASS[challenge.id]||'challenge-generic')+(done?' is-complete':'')+(locked?' is-locked':'')} onClick={()=>!locked&&onStartRun(selected,'stabilization',challenge.id)} aria-label={challenge.name[lang]}><span className="es-map-challenge-icon"/></button>})}
      <button type="button" className="es-map-region-body es-map-focused-body" onClick={()=>onStartRun(selected,'stabilization','none')} aria-label={lang==='ru'?'Стандартная стабилизация':'Standard Stabilization'}><span className="es-map-region-glow"/><span className="es-map-region-surface"/><span className="es-map-region-core"/><span className="es-map-region-ring ring-a"/><span className="es-map-region-ring ring-b"/></button>
      <button type="button" disabled={!loadRegionEndlessUnlock(selected)} className={'es-map-endless-node '+(loadRegionEndlessUnlock(selected)?'is-unlocked':'is-locked')} onClick={()=>loadRegionEndlessUnlock(selected)&&onStartRun(selected,'endless','none')} aria-label={lang==='ru'?'Бесконечное ядро':'Endless Core'}><span className="es-map-endless-symbol"/></button>
    </div>}
   </div><div className="es-map-pan-hint" aria-hidden="true">{lang==='ru'?'СВАЙП ДЛЯ ПЕРЕМЕЩЕНИЯ':'DRAG TO PAN'}</div></div></main>
 </div>;
}
