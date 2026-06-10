#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texture_coord;
layout (location = 2) in vec3 normal;

out vec2 out_texture;
out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float tex_scale_u;
uniform float tex_scale_v;

void main() {
    gl_Position = projection * view * model * vec4(position, 1.0);
    out_texture = vec2(texture_coord.x * tex_scale_u, texture_coord.y * tex_scale_v);
    FragPos = vec3(model * vec4(position, 1.0));
    Normal = mat3(transpose(inverse(model))) * normal;
}