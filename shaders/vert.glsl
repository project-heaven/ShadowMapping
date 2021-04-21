#version 440 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 norm;

uniform mat4x4 transform_mat;
uniform mat4x4 shadow_transform_mat;

out vec3 normal;
out vec3 position;
out vec4 light_space_pos;

void main()
{
	gl_Position = transform_mat * vec4(pos, 1.0);
	light_space_pos = shadow_transform_mat * vec4(pos, 1.0);

	normal = norm;
	position = pos;
}