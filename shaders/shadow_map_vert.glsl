#version 440 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 norm;

uniform mat4x4 transform_mat;

void main()
{
	gl_Position = transform_mat * vec4(pos, 1.0);
}
