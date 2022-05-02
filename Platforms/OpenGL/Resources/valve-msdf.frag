floatdistAlphaMask = baseColor.a;
if (
  OUTLINE &&
  distAlphaMask >= OUTLINEMINVALUE0 &&
  distAlphaMask <= OUTLINEMAXVALUE1
) {
  floatoFactor = 1.0;
  if (distAlphaMask <= OUTLINEMINVALUE1) {
    oFactor = smoothstep(OUTLINEMINVALUE0, OUTLINEMINVALUE1, distAlphaMask);
  } else {
    oFactor = smoothstep(OUTLINEMAXVALUE1, OUTLINEMAXVALUE0, distAlphaMask);
  }
  baseColor = lerp(baseColor, OUTLINECOLOR, oFactor);
}

if (SOFTEDGES) {
  baseColor.a *= smoothstep(SOFTEDGEMIN, SOFTEDGEMAX, distAlphaMask);
} else {
  baseColor.a = distAlphaMask >= 0.5;
}

if (OUTERGLOW) {
  float4glowTexel = tex2D(BaseTextureSampler, i.baseTexCoord.xy + GLOWUVOFFSET);
  float4glowc =
    OUTERGLOWCOLOR *
    smoothstep(OUTERGLOWMINDVALUE, OUTERGLOWMAXDVALUE, glowTexel.a);
  baseColor = lerp(glowc, baseColor, mskUsed);
}
