using System;
using System.Collections.Generic;
using System.Globalization;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private static string NativeFormationName(string type,bool ru)
        {
            switch(type){case "line":return ru?"ЛИНИЯ":"LINE";case "triangle":return ru?"ТРЕУГОЛЬНИК":"TRIANGLE";
                case "square":return ru?"КВАДРАТ":"SQUARE";case "cluster":return ru?"КЛАСТЕР":"CLUSTER";
                case "ring":return ru?"КОЛЬЦО":"RING";case "lattice":return ru?"РЕШЁТКА":"LATTICE";
                case "fractal":return ru?"ФРАКТАЛ":"FRACTAL";default:return type.ToUpperInvariant();}
        }
        private static double NativeXpPlannerMultiplier(V scene)
        {
            var xp=scene["shopUpgrades"]["xp"];double mult=1+(xp.Truth?xp.Number:0)*.05;
            foreach(var id in scene["player"]["artifacts"].Items){var effect=Visual_artifactSystem.Scope["ARTIFACT_META"][id]["effects"]["xpGain"];if(effect.Truth)mult+=effect.Number;}
            return mult;
        }
        private static IReadOnlyList<NativeHudView.TooltipLine> NativeNetworkTooltipLines(string label,V scene,bool ru,V network)
        {
            var activeTypes=new HashSet<string>(StringComparer.Ordinal);foreach(var sphere in scene["spheres"].Items)if(sphere["alive"].Truth)activeTypes.Add(sphere["type"].Text);
            bool isSecondary=network["secondaryFormation"]["type"].Text==label.ToLowerInvariant();
            bool isInactiveDominant=network["dominantFormation"]["active"].Raw is bool active&&!active&&network["dominantFormation"]["type"].Text==label.ToLowerInvariant();
            double scale=isInactiveDominant?0:isSecondary?.5:1;
            string Percent(double value){double scaled=value*scale;return Math.Truncate(scaled)==scaled?scaled.ToString(CultureInfo.InvariantCulture):scaled.ToString("F1",CultureInfo.InvariantCulture);}
            var lines=new List<NativeHudView.TooltipLine>();
            void Add(string activeRu,string restRu,string activeEn,string restEn)=>lines.Add(new NativeHudView.TooltipLine(ru?activeRu:activeEn,ru?restRu:restEn));
  switch (label) {
    case "LINE":
      Add("+" + Percent(10) + "%", isSecondary ? " из полного +10% урона попаданий через сеть" : " урона попаданий через активную сеть", "+" + Percent(10) + "%", isSecondary ? " of the full +10% network hit-damage bonus" : " hit damage through the active network");
      Add("+1", isSecondary ? " пробитие остаётся дискретным сетевым эффектом" : " пробитие для снарядов", "+1", isSecondary ? " pierce remains a discrete network effect" : " projectile pierce");
      if (activeTypes.Contains("prism")) Add("+" + Percent(12) + "%", isSecondary ? " из полного +12% урона луча Призмы" : " урона луча Призмы", "+" + Percent(12) + "%", isSecondary ? " of the full +12% Prism beam damage" : " Prism beam damage");
      break;
    case "TRIANGLE":
      Add((35 * scale).ToString("0.#",CultureInfo.InvariantCulture) + "%", isSecondary ? " из полного 35% фактического урона сетевого импульса" : " фактического урона сетевого импульса", (35 * scale).ToString("0.#",CultureInfo.InvariantCulture) + "%", isSecondary ? " of the full 35% actual-damage network pulse" : " actual damage of the network pulse");
      Add("каждый 3-й удар", isSecondary ? " сферы создаёт импульс" : " сферы создаёт импульс · радиус 88", "every 3rd hit", isSecondary ? " creates the partial pulse" : " creates the pulse · radius 88");
      Add(isSecondary ? "50%" : "100%", isSecondary ? " эффективности вторичного слоя" : " эффективности доминирующего слоя", isSecondary ? "50%" : "100%", isSecondary ? " secondary-layer effectiveness" : " dominant-layer effectiveness");
      if (activeTypes.Contains("pulse") || activeTypes.Contains("prism")) Add(isSecondary ? "50%" : "100%", " сетевого вклада в Резонанс через Треугольник", isSecondary ? "50%" : "100%", " network Resonance contribution through Triangle");
      break;
    case "CLUSTER":
      Add("-" + Percent(10) + "%", isSecondary ? " к интервалу атак вместо полного -10%" : " к интервалу атак сфер", "-" + Percent(10) + "%", isSecondary ? " Sphere attack interval instead of the full -10%" : " Sphere attack interval");
      if (activeTypes.Contains("orbital")) Add("+" + Percent(8) + "%", isSecondary ? " к урону спутников вместо полного +8%" : " к урону спутников", "+" + Percent(8) + "%", isSecondary ? " satellite damage instead of the full +8%" : " satellite damage");
      if (activeTypes.Contains("gravity")) Add("+" + Percent(20) + "%", isSecondary ? " к силе притяжения вместо полного +20%" : " к силе притяжения", "+" + Percent(20) + "%", isSecondary ? " pull strength instead of the full +20%" : " pull strength");
      if (activeTypes.Contains("pulse")) Add("+" + Percent(6) + "%", isSecondary ? " к радиусу импульса вместо полного +6%" : " к радиусу импульса", "+" + Percent(6) + "%", isSecondary ? " pulse radius instead of the full +6%" : " pulse radius");
      break;
    case "SQUARE":
      Add(isSecondary ? "8-й" : "4-й", " удар: +1 заряд щита", isSecondary ? "8th" : "4th", " hit: +1 shield charge");
      Add(isSecondary ? "50%" : "100%", " эффективности оборонительной формации", isSecondary ? "50%" : "100%", " defensive formation effectiveness");
      break;
    case "RING":
      if (activeTypes.Contains("orbital")) Add("+" + Percent(12) + "%", isSecondary ? " к радиусу орбиты вместо полного +12%" : " к радиусу орбиты", "+" + Percent(12) + "%", isSecondary ? " orbit radius instead of the full +12%" : " orbit radius");
      Add(isSecondary ? "50%" : "+1", isSecondary ? " шанс дополнительного отражённого луча" : " дополнительный отражённый луч", isSecondary ? "50%" : "+1", isSecondary ? " chance for the extra reflected beam" : " extra reflected beam");
      Add(isSecondary ? "50%" : "100%", " вклада Импульсного Резонатора в Резонанс", isSecondary ? "50%" : "100%", " Pulse Resonator network contribution to Resonance");
      break;
    case "LATTICE":
      if (activeTypes.Contains("prism")) Add("+" + Percent(8) + "%", isSecondary ? " к урону луча Призмы вместо полного +8%" : " к урону луча Призмы", "+" + Percent(8) + "%", isSecondary ? " Prism beam damage instead of the full +8%" : " Prism beam damage");
      Add(isSecondary ? "50%" : "100%", " вклада Импульсного Резонатора в Резонанс", isSecondary ? "50%" : "100%", " Pulse Resonator network contribution to Resonance");
      break;
    case "FRACTAL":
      Add("+" + Percent(15) + "%", isSecondary ? " к урону попаданий сфер вместо полного +15%" : " к урону попаданий сфер", "+" + Percent(15) + "%", isSecondary ? " Sphere hit damage instead of the full +15%" : " Sphere hit damage");
      break;
    default: break;
  }
            return lines;
        }
    }
}
