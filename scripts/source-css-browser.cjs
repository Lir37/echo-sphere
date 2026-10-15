exports.stylesheet=data=>data.rules.map(rule=>{let text=rule.selector+'{'+rule.declarations.map(d=>d.property+':'+d.value+(d.important?'!important':'')+';').join('')+'}';for(const condition of [...rule.conditions].reverse())text='@'+condition.kind+' '+condition.value+'{'+text+'}';return text;}).join('\n')+'\n'+Object.entries(data.keyframes).map(([name,frames])=>'@keyframes '+name+'{'+frames.map(frame=>frame.offset+'{'+frame.declarations.map(d=>d.property+':'+d.value+';').join('')+'}').join('')+'}').join('\n');
// Playwright screenshots are standard, non-interlaced 8-bit RGB(A) PNGs. Decode
// the scanline filters locally to keep fixture regeneration dependency-free.
exports.png=buffer=>{
 const zlib=require('node:zlib');let offset=8,width,height,channels,parts=[];
 while(offset<buffer.length){const size=buffer.readUInt32BE(offset),kind=buffer.toString('ascii',offset+4,offset+8),data=buffer.subarray(offset+8,offset+8+size);offset+=size+12;
  if(kind==='IHDR'){width=data.readUInt32BE(0);height=data.readUInt32BE(4);if(data[8]!==8||data[12]!==0||![2,6].includes(data[9]))throw Error('Unsupported browser PNG format');channels=data[9]===6?4:3;}
  if(kind==='IDAT')parts.push(data);
 }
 const raw=zlib.inflateSync(Buffer.concat(parts));const stride=width*channels,decoded=Buffer.alloc(stride*height);let source=0;
 function paeth(a,b,c){const p=a+b-c,pa=Math.abs(p-a),pb=Math.abs(p-b),pc=Math.abs(p-c);return pa<=pb&&pa<=pc?a:pb<=pc?b:c;}
 for(let y=0;y<height;y++){const filter=raw[source++];for(let x=0;x<stride;x++){const at=y*stride+x,a=x>=channels?decoded[at-channels]:0,b=y?decoded[at-stride]:0,c=y&&x>=channels?decoded[at-stride-channels]:0;decoded[at]=(raw[source++]+(filter===0?0:filter===1?a:filter===2?b:filter===3?Math.floor((a+b)/2):filter===4?paeth(a,b,c):(()=>{throw Error('Unknown PNG filter');})()))&255;}}
 const pixels=Buffer.alloc(width*height*4);for(let i=0;i<width*height;i++){decoded.copy(pixels,i*4,i*channels,i*channels+3);pixels[i*4+3]=channels===4?decoded[i*4+3]:255;}return{width,height,pixels};
};
exports.encodePng=(width,height,pixels)=>{
 const zlib=require('node:zlib');function chunk(name,data){const tag=Buffer.from(name),size=Buffer.alloc(4),crc=Buffer.alloc(4);size.writeUInt32BE(data.length);crc.writeUInt32BE(zlib.crc32(Buffer.concat([tag,data])));return Buffer.concat([size,tag,data,crc]);}
 const header=Buffer.alloc(13);header.writeUInt32BE(width);header.writeUInt32BE(height,4);header[8]=8;header[9]=6;const rows=Buffer.alloc((width*4+1)*height);for(let y=0;y<height;y++)pixels.copy(rows,y*(width*4+1)+1,y*width*4,(y+1)*width*4);return Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]),chunk('IHDR',header),chunk('IDAT',zlib.deflateSync(rows)),chunk('IEND',Buffer.alloc(0))]);
};
