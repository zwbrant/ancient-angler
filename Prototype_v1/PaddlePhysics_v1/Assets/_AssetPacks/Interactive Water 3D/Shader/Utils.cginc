#ifndef IW3D_UTILS
#define IW3D_UTILS

// encode float to RGBA
float4 EncodeHeightmap (float f)
{
	half h = f;
	half positive = f > 0 ? f : 0;
	half negative = f < 0 ? -f : 0;
	
	float4 c = 0;
	c.r = positive;
	c.g = negative;
	c.ba = frac(c.rg * 256);
	c.rg -= c.ba / 256.0;
	return c;
}
// decode RGBA to float
float DecodeHeightmap (float4 rgba)
{
	float4 table = float4(1.0, -1.0, 1.0 / 256.0, -1.0 / 256.0);
	return dot(rgba, table);
}
#define HEIGHT_MAP_READ(t, uv)  DecodeHeightmap(tex2D(t, uv))
#define HEIGHT_MAP_WRITE(h)     EncodeHeightmap(h)

#endif

