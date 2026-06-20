shader_type sky;

uniform vec3 background_color : source_color = vec3(0.005, 0.007, 0.012);

uniform vec3 nebula_color_a : source_color = vec3(0.15, 0.05, 0.25);
uniform vec3 nebula_color_b : source_color = vec3(0.02, 0.10, 0.20);

uniform float nebula_strength = 0.35;

uniform float star_density = 450.0;
uniform float star_threshold = 0.990;
uniform float star_size = 0.2;
uniform float star_brightness = 10.0;

float hash(vec2 p)
{
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float hash3(vec3 p)
{
	p = fract(p * 0.3183099 + vec3(0.1, 0.2, 0.3));
	p *= 17.0;
	return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float noise(vec3 p)
{
	vec3 i = floor(p);
	vec3 f = fract(p);

	f = f * f * (3.0 - 2.0 * f);

	float n000 = hash3(i + vec3(0,0,0));
	float n100 = hash3(i + vec3(1,0,0));
	float n010 = hash3(i + vec3(0,1,0));
	float n110 = hash3(i + vec3(1,1,0));
	float n001 = hash3(i + vec3(0,0,1));
	float n101 = hash3(i + vec3(1,0,1));
	float n011 = hash3(i + vec3(0,1,1));
	float n111 = hash3(i + vec3(1,1,1));

	float x00 = mix(n000, n100, f.x);
	float x10 = mix(n010, n110, f.x);
	float x01 = mix(n001, n101, f.x);
	float x11 = mix(n011, n111, f.x);

	float y0 = mix(x00, x10, f.y);
	float y1 = mix(x01, x11, f.y);

	return mix(y0, y1, f.z);
}

float fbm(vec3 p)
{
	float value = 0.0;
	float amplitude = 0.5;

	for (int i = 0; i < 5; i++)
	{
	    value += noise(p) * amplitude;
	    p *= 2.0;
	    amplitude *= 0.5;
	}

	return value;
}

void sky()
{
	vec3 dir = normalize(EYEDIR);

	vec3 color = background_color;

	float nebula = fbm(dir * 3.0 + vec3(3.0, 1.0, 2.0));
	nebula = smoothstep(0.4, 0.85, nebula);

	vec3 nebula_color =
	    mix(nebula_color_a, nebula_color_b, fbm(dir * 2.0));

	color += nebula_color * nebula * nebula_strength;

	vec2 uv = SKY_COORDS;

	vec2 grid = uv * star_density;
	vec2 cell = floor(grid);
	vec2 local_uv = fract(grid) - 0.5;

	float rnd = hash(cell);

	if (rnd > star_threshold)
	{
	    float dist = length(local_uv);

	    float star =
	        smoothstep(star_size, 0.0, dist);

	    float intensity =
	        mix(0.5, 1.0,
	            (rnd - star_threshold) /
	            (1.0 - star_threshold));

	    color += vec3(star * intensity * star_brightness);
	}

	COLOR = color;
}
