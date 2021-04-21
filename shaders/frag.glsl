#version 440 core

out vec4 color;

in vec3 normal;
in vec3 position;
in vec4 light_space_pos;

layout(binding = 0)uniform sampler2DShadow shadow_map;
layout(binding = 1, rgb32f)uniform sampler2D normals;

uniform vec3 light_dir;

void main()
{
	vec4 tex_coords = light_space_pos;
	tex_coords.xyz /= tex_coords.w;
	tex_coords.xyz = (tex_coords.xyz + vec3(1.0)) * 0.5;

	tex_coords.z += 0.005 * sign(dot(normal, light_dir)) * length(cross(normal, light_dir));

	vec2 tex_size = textureSize(shadow_map, 0);
	vec2 pixel_size = vec2(1.0) / tex_size;

	float depth;

	// PCF
	for(int x = -2; x <= 2; x++)
		for(int y = -2; y <= 2; y++)
		{
			vec3 tex = tex_coords.xyz + vec3(x, y, 0) * vec3(pixel_size, 0.0);
			depth += texture(shadow_map, tex);
		}

	depth /= 25.0;

	color = vec4(vec3(depth), 1.0);
}
