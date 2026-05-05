#version 330 core

attribute vec3 position;
attribute vec2 texture_coord;
varying vec2 out_texture;
		
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float tex_scale_u;
uniform float tex_scale_v;

void main(){
	gl_Position = projection * view * model * vec4(position,1.0);
	out_texture = vec2(texture_coord.x * tex_scale_u, texture_coord.y * tex_scale_v);
}