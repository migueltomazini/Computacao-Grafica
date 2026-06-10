#version 330 core

// -----------------------------------------------------------------------------
// Variáveis de Entrada do Pipeline Geométrico
// -----------------------------------------------------------------------------
in vec2 out_texture;
in vec3 FragPos;
in vec3 Normal;

// -----------------------------------------------------------------------------
// Variáveis de Saída para o Monitor
// -----------------------------------------------------------------------------
out vec4 FragColor;

// -----------------------------------------------------------------------------
// Uniforms de Componentes Globais e Câmera
// -----------------------------------------------------------------------------
uniform sampler2D objTexture;
uniform vec3 viewPos;

// -----------------------------------------------------------------------------
// Estrutura de Fonte de Luz Dinâmica
// -----------------------------------------------------------------------------
struct Light {
    vec3 position;
    vec3 color;
    int type;        
    int is_on;       
    vec3 direction;  
    float cutOff;    
    float outerCutOff; 
};

uniform Light lights[5];

// -----------------------------------------------------------------------------
// Parâmetros Físicos dos Materiais e Interação
// -----------------------------------------------------------------------------
uniform float ambientIntensity;
uniform int ambientActive;

uniform float kd;                  
uniform float ks;                  
uniform float ns;                  
uniform int is_light_source;       
uniform int obj_location;          
uniform vec3 emit_color;           

uniform float global_kd;           
uniform float global_ks;           

void main() {
    // Amostragem da Cor Nativa a partir das Coordenadas UV
    vec4 texColor = texture(objTexture, out_texture);
    
    // Tratamento de Fontes Emissivas
    if (is_light_source == 1) {
        FragColor = vec4(emit_color, 1.0); 
        return;
    }

    vec3 norm = normalize(Normal);
    
    // CORREÇÃO DEFINITIVA: Two-Sided Lighting para paredes sem espessura
    // Se a câmera estiver vendo o polígono por trás (lado de fora), inverte a normal.
    // Isso garante que a luz de dentro bata de "costas" e não vaze para o exterior.
    if (!gl_FrontFacing) {
        norm = -norm;
    }

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 result = vec3(0.0);
    
    // Composição do Fator Ambiente
    if (ambientActive == 1) {
        result += ambientIntensity * vec3(texColor);
    }
    
    // Iteração e Resolução da Equação de Blinn-Phong
    for(int i = 0; i < 5; i++) {
        if (lights[i].is_on == 0) continue; 
        
        if (obj_location == 0 && lights[i].type == 1) continue; 
        
        if (obj_location == 1 && lights[i].type == 0) {
            if (FragPos.z > 0.0 && lights[i].position.z < 0.0) {
                float t = (0.0 - FragPos.z) / (lights[i].position.z - FragPos.z);
                float x_int = FragPos.x + t * (lights[i].position.x - FragPos.x);
                float y_int = FragPos.y + t * (lights[i].position.y - FragPos.y);
                
                if (x_int < -6.0 || x_int > 6.0 || y_int > 5.0 || y_int < 0.0) continue; 
            } else {
                continue; 
            }
        }
        
        vec3 lightDir = normalize(lights[i].position - FragPos);
        
        float spotIntensity = 1.0;
        if (lights[i].cutOff > -1.0) {
            float theta = dot(lightDir, normalize(-lights[i].direction));
            float epsilon = lights[i].cutOff - lights[i].outerCutOff;
            spotIntensity = clamp((theta - lights[i].outerCutOff) / epsilon, 0.0, 1.0);
        }
        
        float dist = length(lights[i].position - FragPos);
        float attenuation = 1.0 / (1.0 + 0.045 * dist + 0.0075 * (dist * dist));
        
        // 1. Etapa Difusa 
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = (kd * global_kd) * diff * lights[i].color * spotIntensity;
        
        // 2. Etapa Especular - Blinn-Phong
        float spec = 0.0;
        // O brilho especular requer que a face esteja recebendo luz diretamente
        if (diff > 0.0) {
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            spec = pow(max(dot(norm, halfwayDir), 0.0), ns);
        }
        vec3 specular = (ks * global_ks) * spec * lights[i].color * spotIntensity;
        
        // Composição Final
        result += (diffuse * vec3(texColor) + specular) * attenuation;
    }
    
    // Entrega Final do Pixel Renderizado
    FragColor = vec4(result, texColor.a);
}