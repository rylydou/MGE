float distAlphaMask = u_baseColor.a;
if (
  u_outline &&
  distAlphaMask >= u_outlineMinValue0 &&
  distAlphaMask <= u_outlineMaxValue1
) {
  floatoFactor = 1.0;
  if (distAlphaMask <= u_outlineMinValue1) {
    oFactor = smoothstep(u_outlineMinValue0, u_outlineMinValue1, distAlphaMask);
  } else {
    oFactor = smoothstep(u_outlineMaxValue1, u_outlineMaxValue0, distAlphaMask);
  }
  u_baseColor = lerp(u_baseColor, u_outlineColor, oFactor);
}

if (u_softEdges) {
  u_baseColor.a *= smoothstep(u_softEdgeMin, u_softEdgeMax, distAlphaMask);
} else {
  u_baseColor.a = distAlphaMask >= 0.5;
}

if (u_outerGlow) {
  float4glowTexel = tex2D(BaseTextureSampler, i.baseTexCoord.xy + u_glowUvOffset);
  float4glowc =
    u_outerGlowColor *
    smoothstep(u_outerGlowMinDValue, u_outerGlowMaxDValue, glowTexel.a);
  u_baseColor = lerp(glowc, u_baseColor, mskUsed);
}
